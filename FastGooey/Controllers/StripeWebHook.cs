using FastGooey.Models;
using FastGooey.Models.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Stripe;

namespace FastGooey.Controllers;

public class StripeWebHook(
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
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

        // Fallback to email lookup
        string customerEmail = await GetCustomerEmailAsync(subscription);
        if (string.IsNullOrEmpty(customerEmail)) return null;

        user = await userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == customerEmail.ToUpperInvariant());
        if (user is not null)
        {
            user.StripeCustomerId = customerId;
            await userManager.UpdateAsync(user);
        }

        return user;
    }
    
    private async Task<string> GetCustomerEmailAsync(Subscription subscription)
    {
        if (subscription.Customer is Customer customerObj)
        {
            return customerObj.Email;
        }

        var customerService = new CustomerService();
        var customer = await customerService.GetAsync(subscription.CustomerId);
        return customer.Email;
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
}