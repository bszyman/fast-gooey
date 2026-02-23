using FastGooey.Models;
using FastGooey.Models.Configuration;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Stripe;
using System.Security.Cryptography;

namespace FastGooey.Controllers;

public class StripeWebHook(
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    EmailerService emailerService,
    ILogger<StripeWebHook> logger) :
    Controller
{
    private readonly StripeConfigurationModel? _config = configuration.GetSection("Stripe").Get<StripeConfigurationModel>()
                                                         ?? throw new InvalidOperationException("Stripe configuration is missing");

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        string json;
        try
        {
            json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read request body");
            return BadRequest("Invalid request body");
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return BadRequest("Empty request body");
        }

        try
        {
            // Validate config early
            if (string.IsNullOrEmpty(_config.SecretKey))
            {
                throw new InvalidOperationException("Stripe secret key is missing");
            }

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _config.SecretKey
            );

            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription is null)
            {
                logger.LogWarning("Event data is not a subscription object");
                return BadRequest("Invalid event data");
            }

            // Handle the event in a separate method
            await HandleStripeEventAsync(stripeEvent, subscription);
            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook error");
            return BadRequest("Webhook signature verification failed");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Configuration error");
            return StatusCode(500, "Internal configuration error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task HandleStripeEventAsync(Event stripeEvent, Subscription subscription)
    {
        var user = await FindOrCreateUserAsync(subscription);
        if (user is null)
        {
            logger.LogWarning($"User not found for subscription {subscription.Id}");
            return; // Acknowledge but skip processing
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.CustomerSubscriptionCreated:
            case EventTypes.CustomerSubscriptionResumed:
            case EventTypes.CustomerSubscriptionUpdated:
                await RecalculateUserSubscriptionAsync(user, subscription.CustomerId, subscription.Id);
                logger.LogInformation($"Recalculated subscription entitlements for user {user.Id}");
                break;
            case EventTypes.CustomerSubscriptionPaused:
            case EventTypes.CustomerSubscriptionDeleted:
                await RecalculateUserSubscriptionAsync(user, subscription.CustomerId);
                logger.LogInformation($"Recalculated subscription entitlements after pause/delete for user {user.Id}");
                break;
            default:
                logger.LogWarning($"Unhandled Stripe event type: {stripeEvent.Type}");
                break;
        }
    }

    private async Task<ApplicationUser?> FindOrCreateUserAsync(Subscription subscription)
    {
        var customerId = subscription.CustomerId;

        // First, try by StripeCustomerId
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == customerId);
        if (user is not null) return user;

        var customer = await GetCustomerAsync(subscription);
        if (customer is null || string.IsNullOrWhiteSpace(customer.Email))
        {
            return null;
        }

        var normalizedEmail = userManager.NormalizeEmail(customer.Email);
        user = await userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (user is not null)
        {
            user.StripeCustomerId = customerId;
            await userManager.UpdateAsync(user);
            return user;
        }

        var (firstName, lastName) = SplitName(customer.Name, customer.Email);
        var password = GenerateRandomPassword();

        var newUser = new ApplicationUser
        {
            UserName = customer.Email,
            Email = customer.Email,
            FirstName = firstName,
            LastName = lastName,
            StripeCustomerId = customerId,
            StripeSubscriptionId = null,
            SubscriptionLevel = SubscriptionLevel.Explorer,
            StandardWorkspaceAllowance = 0
        };

        var result = await userManager.CreateAsync(newUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create user from Stripe webhook: {Errors}", errors);
            return null;
        }

        try
        {
            await emailerService.SendStripeWelcomeEmail(newUser, password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Stripe welcome email for user {UserId}", newUser.Id);
        }

        return newUser;
    }

    private async Task<Customer?> GetCustomerAsync(Subscription subscription)
    {
        if (subscription.Customer is Customer customerObj)
        {
            return customerObj;
        }

        var customerService = new CustomerService();
        return await customerService.GetAsync(subscription.CustomerId);
    }

    private async Task RecalculateUserSubscriptionAsync(ApplicationUser user, string customerId, string? preferredSubscriptionId = null)
    {
        var subscriptionService = new SubscriptionService();
        var subscriptions = subscriptionService.ListAutoPaging(new SubscriptionListOptions
        {
            Customer = customerId,
            Status = "all",
            Limit = 100
        }).ToList();

        var entitlementSubscriptions = subscriptions
            .Where(HasEntitlementStatus)
            .ToList();

        var highestLevel = SubscriptionLevel.Explorer;
        var standardWorkspaceAllowance = 0;
        string? selectedSubscriptionId = null;

        foreach (var stripeSubscription in entitlementSubscriptions)
        {
            var itemLevels = GetItemLevels(stripeSubscription).ToList();
            if (itemLevels.Count == 0)
            {
                continue;
            }

            var subscriptionLevel = itemLevels.Max();
            if (subscriptionLevel > highestLevel)
            {
                highestLevel = subscriptionLevel;
                selectedSubscriptionId = stripeSubscription.Id;
            }

            standardWorkspaceAllowance += GetStandardWorkspaceAllowance(stripeSubscription);
        }

        if (highestLevel == SubscriptionLevel.Agency)
        {
            standardWorkspaceAllowance = 0;
        }

        if (!string.IsNullOrWhiteSpace(preferredSubscriptionId) &&
            entitlementSubscriptions.Any(x => x.Id == preferredSubscriptionId))
        {
            selectedSubscriptionId = preferredSubscriptionId;
        }

        user.StripeCustomerId = customerId;
        user.StripeSubscriptionId = selectedSubscriptionId;
        user.SubscriptionLevel = highestLevel;
        user.StandardWorkspaceAllowance = standardWorkspaceAllowance;
        user.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        await userManager.UpdateAsync(user);
    }

    private SubscriptionLevel MapPlanToLevel(string priceId)
    {
        if (_config!.Prices is null)
        {
            throw new InvalidOperationException("Stripe price configuration is missing");
        }

        var levelName = _config!
            .Prices
            .FirstOrDefault(kvp => kvp.Value == priceId).Key;

        return Enum.TryParse<SubscriptionLevel>(levelName, out var level) ?
            level :
            SubscriptionLevel.Explorer;
    }

    private IEnumerable<SubscriptionLevel> GetItemLevels(Subscription subscription)
    {
        foreach (var item in subscription.Items?.Data ?? [])
        {
            var priceId = item.Price?.Id;
            if (string.IsNullOrWhiteSpace(priceId))
            {
                continue;
            }

            yield return MapPlanToLevel(priceId);
        }
    }

    private static bool HasEntitlementStatus(Subscription subscription)
    {
        return subscription.Status == "active" ||
               subscription.Status == "trialing" ||
               subscription.Status == "past_due" ||
               subscription.Status == "unpaid";
    }

    private int GetStandardWorkspaceAllowance(Subscription subscription)
    {
        var itemLevels = GetItemLevels(subscription).ToList();
        if (itemLevels.Count == 0)
        {
            return 0;
        }

        var subscriptionLevel = itemLevels.Max();
        if (subscriptionLevel == SubscriptionLevel.Agency)
        {
            return 0;
        }

        if (subscriptionLevel != SubscriptionLevel.Standard)
        {
            return 0;
        }

        var standardPriceId = _config?.Prices?.GetValueOrDefault(nameof(SubscriptionLevel.Standard));
        if (string.IsNullOrWhiteSpace(standardPriceId))
        {
            return 1;
        }

        var standardItem = (subscription.Items?.Data ?? [])
            .FirstOrDefault(item =>
            item.Price?.Id == standardPriceId);
        
        if (standardItem is null)
        {
            return 1;
        }

        var quantity = standardItem.Quantity > 1 ? standardItem.Quantity : 1;
        return quantity <= 0 ? 1 : (int)quantity;
    }

    private static (string FirstName, string LastName) SplitName(string? fullName, string email)
    {
        var trimmed = (fullName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            var prefix = email.Split('@')[0];
            return (prefix, string.Empty);
        }

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string all = upper + lower + digits;

        var chars = new List<char>
        {
            GetRandomChar(upper),
            GetRandomChar(lower),
            GetRandomChar(digits)
        };

        for (var i = chars.Count; i < length; i++)
        {
            chars.Add(GetRandomChar(all));
        }

        Shuffle(chars);
        return new string(chars.ToArray());
    }

    private static char GetRandomChar(string allowedChars)
    {
        return allowedChars[RandomNumberGenerator.GetInt32(allowedChars.Length)];
    }

    private static void Shuffle(IList<char> chars)
    {
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
