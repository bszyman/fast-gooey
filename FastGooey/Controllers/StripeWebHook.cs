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
    ILogger<StripeWebHook> logger): 
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
                await UpdateUserSubscriptionAsync(user, subscription);
                logger.LogInformation($"Activated/resumed subscription for user {user.Id}");
                break;
            case EventTypes.CustomerSubscriptionPaused:
            case EventTypes.CustomerSubscriptionDeleted:
                await CancelUserSubscriptionAsync(user);
                logger.LogInformation($"Paused/deleted subscription for user {user.Id}");
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
            StripeSubscriptionId = subscription.Id,
            SubscriptionLevel = MapPlanToLevel(subscription.Items.Data[0].Price.Id)
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

    private async Task UpdateUserSubscriptionAsync(ApplicationUser user, Subscription subscription)
    {
        user.StripeCustomerId = subscription.CustomerId;
        user.StripeSubscriptionId = subscription.Id;
        user.SubscriptionLevel = MapPlanToLevel(subscription.Items.Data[0].Price.Id);
        user.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        await userManager.UpdateAsync(user);
    }

    private async Task CancelUserSubscriptionAsync(ApplicationUser user)
    {
        user.SubscriptionLevel = SubscriptionLevel.Explorer;
        user.StripeSubscriptionId = null;
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
