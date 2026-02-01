using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Fido2NetLib;
using Fido2NetLib.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NodaTime;

namespace FastGooey.Controllers;

public class Auth(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ITurnstileValidatorService turnstileValidator,
    EmailerService emailSender,
    IDistributedCache cache,
    Fido2 fido2,
    IClock clock) :
    Controller
{
    private const string PasskeyRegistrationCachePrefix = "passkey.register.";
    private const string PasskeyAssertionCachePrefix = "passkey.assert.";
    private static readonly TimeSpan PasskeyCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MagicLinkLifetime = TimeSpan.FromMinutes(15);

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        if (!await turnstileValidator.ValidateFormRequest(model.TurnstileToken))
        {
            ModelState.AddModelError(
                "Request validation failed.",
                "Request validation failed. Refresh the page and try logging in again."
                );
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false // TODO: Revist
        );

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // render workspace succeeded partial
            HttpContext.Response.Headers["HX-Redirect"] = "/workspaces";
            return PartialView("~/Views/Auth/LoginSucceeded.cshtml");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        // render form partial
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestMagicLink(
        MagicLinkRequestViewModel model,
        string? returnUrl = null,
        string? redirectTo = null)
    {
        if (!ModelState.IsValid)
        {
            TempData["MagicLinkSent"] = true;
            return RedirectToMagicLinkTarget(redirectTo, returnUrl);
        }

        if (!await turnstileValidator.ValidateFormRequest(model.TurnstileToken))
        {
            TempData["MagicLinkSent"] = true;
            return RedirectToMagicLinkTarget(redirectTo, returnUrl);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var token = CreateMagicLinkToken();
            var tokenHash = HashMagicLinkToken(token);
            var now = clock.GetCurrentInstant();

            dbContext.MagicLinkTokens.Add(new MagicLinkToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now + Duration.FromTimeSpan(MagicLinkLifetime)
            });
            await dbContext.SaveChangesAsync();

            var magicLink = Url.Action(
                nameof(MagicLinkCallback),
                "Auth",
                new { token, email = user.Email, returnUrl },
                Request.Scheme);

            if (!string.IsNullOrWhiteSpace(magicLink))
            {
                await emailSender.SendMagicLinkEmail(user, magicLink);
            }
        }

        TempData["MagicLinkSent"] = true;
        return RedirectToMagicLinkTarget(redirectTo, returnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> MagicLinkCallback(string token, string email, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            return RedirectToAction("Index", "Home");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return RedirectToAction("Index", "Home");

        var tokenHash = HashMagicLinkToken(token);
        var now = clock.GetCurrentInstant();

        var storedToken = await dbContext.MagicLinkTokens
            .Where(t => t.UserId == user.Id && t.TokenHash == tokenHash)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (storedToken is null || storedToken.UsedAt is not null || storedToken.ExpiresAt < now)
        {
            TempData["MagicLinkInvalid"] = true;
            return RedirectToAction("Index", "Home");
        }

        storedToken.UsedAt = now;
        await dbContext.SaveChangesAsync();

        await signInManager.SignInAsync(user, isPersistent: false);

        return string.IsNullOrWhiteSpace(returnUrl) ?
            RedirectToAction("Index", "WorkspaceSelector") :
            LocalRedirect(returnUrl);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BeginPasskeyRegistration([FromBody] PasskeyRegistrationStartRequest request)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

        var existingCredentials = await dbContext.PasskeyCredentials
            .Where(p => p.UserId == currentUser.Id)
            .ToListAsync();

        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? (currentUser.Email ?? currentUser.UserName ?? "FastGooey User")
            : request.DisplayName.Trim();

        var fidoUser = new Fido2User
        {
            Id = Encoding.UTF8.GetBytes(currentUser.Id),
            Name = currentUser.Email ?? currentUser.UserName ?? currentUser.Id,
            DisplayName = displayName
        };

        var excludeCredentials = existingCredentials
            .Select(p => new Fido2NetLib.Objects.PublicKeyCredentialDescriptor(p.DescriptorId))
            .ToList();

        var options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = excludeCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = Fido2NetLib.Objects.ResidentKeyRequirement.Preferred,
                UserVerification = Fido2NetLib.Objects.UserVerificationRequirement.Preferred
            },
            AttestationPreference = Fido2NetLib.Objects.AttestationConveyancePreference.None
        });

        var requestId = Guid.NewGuid().ToString("N");
        await StorePasskeyOptionsAsync(
            PasskeyRegistrationCachePrefix,
            requestId,
            JsonSerializer.Serialize(options, FidoModelSerializerContext.Default.CredentialCreateOptions));

        return Json(new PasskeyOptionsResponse(requestId, options));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePasskeyRegistration([FromBody] PasskeyRegistrationFinishRequest request)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

        var options = await GetPasskeyOptionsAsync(
            PasskeyRegistrationCachePrefix,
            request.RequestId,
            json => JsonSerializer.Deserialize(json, FidoModelSerializerContext.Default.CredentialCreateOptions));

        if (options is null)
            return BadRequest(new { message = "Passkey registration expired. Please try again." });

        var credential = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = request.AttestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
                !await dbContext.PasskeyCredentials.AnyAsync(
                    p => p.DescriptorId == args.CredentialId,
                    cancellationToken)
        });

        var now = clock.GetCurrentInstant();

        dbContext.PasskeyCredentials.Add(new PasskeyCredential
        {
            UserId = currentUser.Id,
            DescriptorId = credential.Id,
            PublicKey = credential.PublicKey,
            CredentialType = credential.Type.ToString(),
            SignatureCounter = credential.SignCount,
            Aaguid = credential.AaGuid,
            UserHandle = credential.User.Id,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
            CreatedAt = now,
            LastUsedAt = now
        });

        await dbContext.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BeginPasskeyLogin([FromBody] PasskeyAssertionStartRequest request)
    {
        List<Fido2NetLib.Objects.PublicKeyCredentialDescriptor> allowedCredentials = [];

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is not null)
            {
                allowedCredentials = await dbContext.PasskeyCredentials
                    .Where(p => p.UserId == user.Id)
                    .Select(p => new Fido2NetLib.Objects.PublicKeyCredentialDescriptor(p.DescriptorId))
                    .ToListAsync();
            }
        }

        var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCredentials,
            UserVerification = Fido2NetLib.Objects.UserVerificationRequirement.Preferred
        });

        var requestId = Guid.NewGuid().ToString("N");
        await StorePasskeyOptionsAsync(
            PasskeyAssertionCachePrefix,
            requestId,
            JsonSerializer.Serialize(options, FidoModelSerializerContext.Default.AssertionOptions));

        return Json(new PasskeyAssertionOptionsResponse(requestId, options));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePasskeyLogin([FromBody] PasskeyAssertionFinishRequest request, string? returnUrl = null)
    {
        var options = await GetPasskeyOptionsAsync(
            PasskeyAssertionCachePrefix,
            request.RequestId,
            json => JsonSerializer.Deserialize(json, FidoModelSerializerContext.Default.AssertionOptions));

        if (options is null)
            return BadRequest(new { message = "Passkey login expired. Please try again." });

        var storedCredential = await dbContext.PasskeyCredentials
            .FirstOrDefaultAsync(p => p.DescriptorId == request.AssertionResponse.RawId);

        if (storedCredential is null)
            return BadRequest(new { message = "Passkey not recognized." });

        var assertion = await fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = request.AssertionResponse,
            OriginalOptions = options,
            StoredPublicKey = storedCredential.PublicKey,
            StoredSignatureCounter = storedCredential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, cancellationToken) =>
            {
                var credential = await dbContext.PasskeyCredentials
                    .FirstOrDefaultAsync(p => p.DescriptorId == args.CredentialId, cancellationToken);

                return credential?.UserHandle is not null &&
                       args.UserHandle is not null &&
                       credential.UserHandle.SequenceEqual(args.UserHandle);
            }
        });

        storedCredential.SignatureCounter = assertion.SignCount;
        storedCredential.LastUsedAt = clock.GetCurrentInstant();

        await dbContext.SaveChangesAsync();

        var user = await userManager.FindByIdAsync(storedCredential.UserId);
        if (user is null)
            return Unauthorized();

        await signInManager.SignInAsync(user, isPersistent: false);

        var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/workspaces" : returnUrl;
        return Ok(new { redirectUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEmailValidation()
    {
        try
        {
            var currentUser = await userManager.GetUserAsync(User);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(currentUser);
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Auth",
                new { userId = currentUser.Id, token },
                Request.Scheme
            );

            await emailSender.SendVerificationEmail(currentUser, confirmationLink);

            return Ok("<p class='mt-4'>A new verification email has been sent.</p>");
        }
        catch (Exception e)
        {
            return BadRequest($"<p class='mt-4'>${e}</p>");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Home");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? RedirectToAction("Index", "WorkspaceSelector") : View("Error");
    }

    // [HttpGet]
    // public IActionResult Register(string? returnUrl = null)
    // {
    //     ViewData["ReturnUrl"] = returnUrl;
    //     return View();
    // }
    //
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    // {
    //     ViewData["ReturnUrl"] = returnUrl;
    //
    //     if (ModelState.IsValid)
    //     {
    //         var user = new ApplicationUser 
    //         { 
    //             UserName = model.Email, 
    //             Email = model.Email,
    //             FirstName = model.FirstName,
    //             LastName = model.LastName
    //         };
    //
    //         var result = await userManager.CreateAsync(user, model.Password);
    //
    //         if (result.Succeeded)
    //         {
    //             await signInManager.SignInAsync(user, isPersistent: false);
    //             return LocalRedirect(returnUrl ?? "/");
    //         }
    //
    //         foreach (var error in result.Errors)
    //         {
    //             ModelState.AddModelError(string.Empty, error.Description);
    //         }
    //     }
    //
    //     return View(model);
    // }

    [HttpGet]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(
            nameof(ExternalLoginCallback),
            "Auth",
            new { returnUrl }
        );

        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            redirectUrl
        );

        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return RedirectToAction(nameof(Login));

        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
            info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        // If the user does not have an account, create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (email is null)
            return RedirectToAction(nameof(Login));

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user);
        }

        await userManager.AddLoginAsync(user, info);
        await signInManager.SignInAsync(user, isPersistent: false);

        return LocalRedirect(returnUrl ?? "/");
    }

    private static byte[] HashMagicLinkToken(string token)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }

    private static string CreateMagicLinkToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private async Task StorePasskeyOptionsAsync(string prefix, string requestId, string json)
    {
        await cache.SetStringAsync(
            $"{prefix}{requestId}",
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PasskeyCacheDuration
            });
    }

    private async Task<TOptions?> GetPasskeyOptionsAsync<TOptions>(
        string prefix,
        string requestId,
        Func<string, TOptions?> deserialize)
    {
        var key = $"{prefix}{requestId}";
        var json = await cache.GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(json))
            return default;

        await cache.RemoveAsync(key);
        return deserialize(json);
    }

    private IActionResult RedirectToMagicLinkTarget(string? redirectTo, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(redirectTo) && Url.IsLocalUrl(redirectTo))
            return Redirect(redirectTo);

        return RedirectToAction("Index", "Home", new { returnUrl });
    }
}
