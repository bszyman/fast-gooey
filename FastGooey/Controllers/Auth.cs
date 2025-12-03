using System.Security.Claims;
using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class Auth(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITurnstileValidatorService turnstileValidator): 
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
            lockoutOnFailure: true
        );

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");
            
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        return View(model);
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
        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return RedirectToAction(nameof(Login));

        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, 
            info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        // If the user does not have an account, create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (email == null)
            return RedirectToAction(nameof(Login));

        var user = await userManager.FindByEmailAsync(email);
            
        if (user == null)
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