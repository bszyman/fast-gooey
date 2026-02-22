using System.Diagnostics;
using FastGooey.Features.Auth.Login.Models.ViewModels;
using FastGooey.Features.Workspaces.Selector.Controllers;
using FastGooey.Models;
using Microsoft.AspNetCore.Mvc;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;

namespace FastGooey.Controllers;

public class HomeController(
    SignInManager<ApplicationUser> signInManager,
    ITurnstileValidatorService turnstileValidator) : 
    Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToAction(nameof(WorkspaceSelectorController.Index), "WorkspaceSelector");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View("Index", model);

        if (!await turnstileValidator.ValidateFormRequest(model.TurnstileToken))
        {
            ModelState.AddModelError(
                "Request validation failed.",
                "Request validation failed. Refresh the page and try logging in again."
            );

            return View("Index", model);
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

            return RedirectToAction(nameof(WorkspaceSelectorController.Index), "WorkspaceSelector");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        return View("Index", model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}