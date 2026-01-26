using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class SignUpController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITurnstileValidatorService turnstileValidator,
    IEmailSender emailSender): 
    Controller
{
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (!ModelState.IsValid)
            return View(model);
        
        if (!await turnstileValidator.ValidateFormRequest(model.TurnstileToken))
        {
            ModelState.AddModelError(
                "Request validation failed.", 
                "Request validation failed. Refresh the page and try submitting the form again."
            );
            return View(model);
        }

        var user = new ApplicationUser 
        { 
            UserName = model.Email, 
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
            
        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            
            // TODO: Extract this logic out
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Auth",
                new { userId = user.Id, token },
                Request.Scheme
            );
            
            var name = $"{user.FirstName} {user.LastName}";

            const string filePath = "Views/EmailNotifications/emailVerification.handlebars";
            var fileContents = await System.IO.File.ReadAllTextAsync(filePath);

            var template = Handlebars.Compile(fileContents);

            var data = new
            {
                name,
                confirmationLink
            };

            var messageContents = template(data);

            await emailSender.SendEmailAsync(
                user.Email,
                "Welcome to FastGooey! Please verify your email address.",
                messageContents
            );
            
            return LocalRedirect(returnUrl ?? "/"); // TODO: better redirect using redirect action
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}