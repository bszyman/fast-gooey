using FastGooey.Database;
using FastGooey.Features.Auth.SignUp.Models.ViewModels;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Auth.SignUp.Controllers;

public class SignUpController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ITurnstileValidatorService turnstileValidator,
    EmailerService emailSender) :
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
            LastName = model.LastName,
            PasskeyRequired = true
        };

        var result = await userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Auth",
                new { userId = user.Id, token },
                Request.Scheme
            );

            if (!string.IsNullOrWhiteSpace(confirmationLink))
            {
                await emailSender.SendVerificationEmail(user, confirmationLink);
            }

            return RedirectToAction(nameof(Complete), new { returnUrl });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipPasskey(string? returnUrl = null)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return RedirectToAction(nameof(Index));

        currentUser.PasskeyRequired = false;
        await userManager.UpdateAsync(currentUser);

        return string.IsNullOrWhiteSpace(returnUrl) ?
            RedirectToAction("Index", "WorkspaceSelector") :
            LocalRedirect(returnUrl);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Complete(string? returnUrl = null)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return RedirectToAction(nameof(Index));

        var hasPasskey = await dbContext.PasskeyCredentials
            .AnyAsync(p => p.UserId == currentUser.Id);

        if (hasPasskey)
        {
            return string.IsNullOrWhiteSpace(returnUrl) ?
                RedirectToAction("Index", "WorkspaceSelector") :
                LocalRedirect(returnUrl);
        }

        var viewModel = new SignUpCompleteViewModel
        {
            Email = currentUser.Email ?? string.Empty,
            ReturnUrl = returnUrl ?? string.Empty
        };

        return View(viewModel);
    }
}
