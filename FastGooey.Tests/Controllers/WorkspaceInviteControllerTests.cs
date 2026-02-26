using System.Security.Claims;
using FastGooey.Controllers;
using FastGooey.Features.Workspaces.Management.Models.ViewModels;
using FastGooey.Models;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class WorkspaceInviteControllerTests
{
    private sealed class StubUserStore : IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }
        public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);
        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);
        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }
        public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);
        public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }
    }

    private sealed class StubUserManager(ApplicationUser? currentUser, ApplicationUser? knownUser) : UserManager<ApplicationUser>(
        new StubUserStore(),
        null,
        new PasswordHasher<ApplicationUser>(),
        [],
        [],
        new UpperInvariantLookupNormalizer(),
        new IdentityErrorDescriber(),
        null,
        NullLogger<UserManager<ApplicationUser>>.Instance)
    {
        public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        {
            return Task.FromResult(currentUser);
        }

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            if (knownUser is not null && string.Equals(knownUser.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ApplicationUser?>(knownUser);
            }

            return Task.FromResult<ApplicationUser?>(null);
        }
    }

    [Fact]
    public async Task AcceptInvite_AssignsWorkspaceToAuthenticatedInvitedUser()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);

        var workspace = new Workspace { Name = "Workspace", Slug = "workspace" };
        var user = new ApplicationUser { Id = "user-1", Email = "invitee@example.com", UserName = "invitee@example.com" };
        dbContext.Workspaces.Add(workspace);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var dataProtectionProvider = DataProtectionProvider.Create("workspace-invite-tests");
        var protector = dataProtectionProvider.CreateProtector("FastGooey.WorkspaceInvite");
        var token = protector.Protect($"{{\"WorkspaceId\":\"{workspace.PublicId}\",\"FirstName\":\"Invited\",\"LastName\":\"User\",\"Email\":\"invitee@example.com\"}}");

        var controller = new WorkspaceInviteController(dbContext, new StubUserManager(user, user), dataProtectionProvider)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"))
                }
            }
        };

        var result = await controller.AcceptInvite(token);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("WorkspaceSelector", redirect.ControllerName);
        Assert.Null(user.WorkspaceId);
        Assert.True(dbContext.WorkspaceMemberships.Any(m => m.UserId == user.Id && m.WorkspaceId == workspace.Id));
    }

    [Fact]
    public async Task AcceptInvite_RedirectsUnknownUserToSignUp()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var workspace = new Workspace { Name = "Workspace", Slug = "workspace" };
        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();

        var dataProtectionProvider = DataProtectionProvider.Create("workspace-invite-tests");
        var protector = dataProtectionProvider.CreateProtector("FastGooey.WorkspaceInvite");
        var token = protector.Protect($"{{\"WorkspaceId\":\"{workspace.PublicId}\",\"FirstName\":\"Invited\",\"LastName\":\"User\",\"Email\":\"new-user@example.com\"}}");

        var controller = new WorkspaceInviteController(dbContext, new StubUserManager(null, null), dataProtectionProvider)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.AcceptInvite(token);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("SignUp", redirect.ControllerName);
    }

    [Fact]
    public async Task Accept_ReturnsInviteViewForValidToken()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var workspace = new Workspace { Name = "Workspace", Slug = "workspace" };
        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();

        var dataProtectionProvider = DataProtectionProvider.Create("workspace-invite-tests");
        var protector = dataProtectionProvider.CreateProtector("FastGooey.WorkspaceInvite");
        var token = protector.Protect($"{{\"WorkspaceId\":\"{workspace.PublicId}\",\"FirstName\":\"Invited\",\"LastName\":\"User\",\"Email\":\"invitee@example.com\"}}");

        var controller = new WorkspaceInviteController(dbContext, new StubUserManager(null, null), dataProtectionProvider)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Accept(token);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WorkspaceInviteViewModel>(view.Model);
        Assert.Equal(token, model.Token);
        Assert.Equal("Workspace", model.WorkspaceName);
        Assert.Equal("invitee@example.com", model.Email);
    }
}
