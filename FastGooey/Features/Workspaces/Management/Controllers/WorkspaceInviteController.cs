using System.Text.Json;
using FastGooey.Database;
using FastGooey.Features.Workspaces.Management.Models.ViewModels;
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
        var inviteContext = await BuildInviteContextAsync(token);
        if (inviteContext is null)
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        var viewModel = new WorkspaceInviteViewModel
        {
            Token = token,
            WorkspaceName = inviteContext.Workspace.Name,
            Email = inviteContext.Payload.Email,
            FirstName = inviteContext.Payload.FirstName,
            LastName = inviteContext.Payload.LastName
        };

        return View("~/Features/Workspaces/Management/Views/Invite.cshtml", viewModel);
    }

    [HttpPost("accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptInvite(string token)
    {
        var inviteContext = await BuildInviteContextAsync(token);
        if (inviteContext is null)
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        var returnUrl = Url?.Action("Accept", "WorkspaceInvite", new { token }) ??
                        $"/workspace-invites/accept?token={Uri.EscapeDataString(token)}";
        if (User.Identity?.IsAuthenticated != true)
        {
            var existingUser = await userManager.FindByEmailAsync(inviteContext.Payload.Email);
            return existingUser is null
                ? RedirectToAction("Index", "SignUp", new { returnUrl })
                : RedirectToAction("Login", "Auth", new { returnUrl });
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl });
        }

        if (!string.Equals(currentUser.Email, inviteContext.Payload.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(currentUser.FirstName))
        {
            currentUser.FirstName = inviteContext.Payload.FirstName;
        }
        if (string.IsNullOrWhiteSpace(currentUser.LastName))
        {
            currentUser.LastName = inviteContext.Payload.LastName;
        }

        var alreadyMember = await dbContext.WorkspaceMemberships.AnyAsync(m =>
            m.UserId == currentUser.Id &&
            m.WorkspaceId == inviteContext.Workspace.Id);

        if (!alreadyMember)
        {
            dbContext.WorkspaceMemberships.Add(new WorkspaceMembership
            {
                UserId = currentUser.Id,
                WorkspaceId = inviteContext.Workspace.Id
            });
        }

        await dbContext.SaveChangesAsync();

        return RedirectToAction("Index", "WorkspaceSelector");
    }

    private async Task<WorkspaceInviteContext?> BuildInviteContextAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        WorkspaceInvitePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WorkspaceInvitePayload>(_inviteProtector.Unprotect(token));
        }
        catch
        {
            return null;
        }

        if (payload is null || payload.WorkspaceId == Guid.Empty || string.IsNullOrWhiteSpace(payload.Email))
        {
            return null;
        }

        var workspace = await dbContext.Workspaces.FirstOrDefaultAsync(w => w.PublicId == payload.WorkspaceId);
        if (workspace is null)
        {
            return null;
        }

        return new WorkspaceInviteContext(payload, workspace);
    }

    private sealed class WorkspaceInvitePayload
    {
        public Guid WorkspaceId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed record WorkspaceInviteContext(WorkspaceInvitePayload Payload, Workspace Workspace);
}
