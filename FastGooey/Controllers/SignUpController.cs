using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class SignUpController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITurnstileValidatorService turnstileValidator): 
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
            return LocalRedirect(returnUrl ?? "/");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}