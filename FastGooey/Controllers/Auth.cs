using System.Security.Claims;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class Auth(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ITurnstileValidatorService turnstileValidator,
    EmailerService emailSender): 
    Controller
{
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
}