using System.Text.Json;
using FastGooey.Database;
using FastGooey.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[AllowAnonymous]
[Route("workspace-invites")]
public class WorkspaceInviteController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IDataProtectionProvider dataProtectionProvider) : Controller
{
    private readonly IDataProtector _inviteProtector = dataProtectionProvider.CreateProtector("FastGooey.WorkspaceInvite");

    [HttpGet("accept")]
    public async Task<IActionResult> Accept(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        WorkspaceInvitePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WorkspaceInvitePayload>(_inviteProtector.Unprotect(token));
        }
        catch
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        if (payload is null || payload.WorkspaceId == Guid.Empty || string.IsNullOrWhiteSpace(payload.Email))
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        var workspace = await dbContext.Workspaces.FirstOrDefaultAsync(w => w.PublicId == payload.WorkspaceId);
        if (workspace is null)
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        var returnUrl = Url.Action("Accept", "WorkspaceInvite", new { token });
        if (User.Identity?.IsAuthenticated != true)
        {
            var existingUser = await userManager.FindByEmailAsync(payload.Email);
            return existingUser is null
                ? RedirectToAction("Index", "SignUp", new { returnUrl })
                : RedirectToAction("Login", "Auth", new { returnUrl });
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl });
        }

        if (!string.Equals(currentUser.Email, payload.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(currentUser.FirstName))
        {
            currentUser.FirstName = payload.FirstName;
        }
        if (string.IsNullOrWhiteSpace(currentUser.LastName))
        {
            currentUser.LastName = payload.LastName;
        }

        currentUser.WorkspaceId = workspace.Id;
        await dbContext.SaveChangesAsync();

        return RedirectToAction("Index", "WorkspaceSelector");
    }

    private sealed class WorkspaceInvitePayload
    {
        public Guid WorkspaceId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
