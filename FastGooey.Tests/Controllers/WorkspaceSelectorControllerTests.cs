using System.Security.Claims;
using FastGooey.Features.Workspaces.Selector.Controllers;
using FastGooey.Features.Workspaces.Selector.Models;
using FastGooey.Features.Workspaces.Selector.Models.FormModels;
using FastGooey.Features.Workspaces.Selector.Models.ViewModels.WorkspaceSelector;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class WorkspaceSelectorControllerTests
{
    private sealed class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    private sealed class StubUserStore : IUserStore<ApplicationUser>
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
    }

    private sealed class StubUserManager(ApplicationUser currentUser) : UserManager<ApplicationUser>(
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
            return Task.FromResult<ApplicationUser?>(currentUser);
        }
    }

    [Fact]
    public async Task Index_DoesNotCountInvitedWorkspacesAgainstOwnedLimits()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var owner = new ApplicationUser
        {
            Id = "owner-1",
            UserName = "owner@example.com",
            Email = "owner@example.com",
            EmailConfirmed = true
        };
        var currentUser = new ApplicationUser
        {
            Id = "user-1",
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            SubscriptionLevel = SubscriptionLevel.Standard
        };
        var invitedExplorer = new Workspace
        {
            Name = "Owner Explorer",
            Slug = "owner-explorer",
            IsExplorer = true,
            OwnerUserId = owner.Id
        };
        var invitedStandard = new Workspace
        {
            Name = "Owner Standard",
            Slug = "owner-standard",
            IsExplorer = false,
            OwnerUserId = owner.Id
        };

        dbContext.Users.AddRange(owner, currentUser);
        dbContext.Workspaces.AddRange(invitedExplorer, invitedStandard);
        await dbContext.SaveChangesAsync();

        dbContext.WorkspaceMemberships.AddRange(
            new WorkspaceMembership { UserId = currentUser.Id, WorkspaceId = invitedExplorer.Id },
            new WorkspaceMembership { UserId = currentUser.Id, WorkspaceId = invitedStandard.Id });
        await dbContext.SaveChangesAsync();

        var controller = new WorkspaceSelectorController(
            new StubKeyValueService(),
            dbContext,
            new ConfigurationBuilder().Build(),
            new StubUserManager(currentUser))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, currentUser.Id)], "test"))
                }
            }
        };

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WorkspaceSelectorViewModel>(view.Model);
        Assert.True(model.CanCreateExplorerWorkspace);
        Assert.Equal(0, model.OwnedStandardWorkspaceCount);
        Assert.Equal(1, model.RemainingStandardWorkspaceSlots);
    }

    [Fact]
    public async Task SaveNewWorkspace_AllowsExplorerCreationWhenUserOnlyInvitedToExplorer()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var owner = new ApplicationUser
        {
            Id = "owner-1",
            UserName = "owner@example.com",
            Email = "owner@example.com",
            EmailConfirmed = true
        };
        var currentUser = new ApplicationUser
        {
            Id = "user-1",
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true
        };
        var invitedExplorer = new Workspace
        {
            Name = "Owner Explorer",
            Slug = "owner-explorer",
            IsExplorer = true,
            OwnerUserId = owner.Id
        };

        dbContext.Users.AddRange(owner, currentUser);
        dbContext.Workspaces.Add(invitedExplorer);
        await dbContext.SaveChangesAsync();

        dbContext.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            UserId = currentUser.Id,
            WorkspaceId = invitedExplorer.Id
        });
        await dbContext.SaveChangesAsync();

        var controller = new WorkspaceSelectorController(
            new StubKeyValueService(),
            dbContext,
            new ConfigurationBuilder().Build(),
            new StubUserManager(currentUser))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, currentUser.Id)], "test"))
                }
            }
        };

        var result = await controller.SaveNewWorkspace(new CreateWorkspace
        {
            WorkspaceName = "My Explorer",
            WorkspacePlan = WorkspacePlan.Explorer
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("WorkspaceSelector", redirect.ControllerName);
        Assert.Equal(1, dbContext.Workspaces.Count(w => w.OwnerUserId == currentUser.Id && w.IsExplorer));
    }

    [Fact]
    public async Task SaveNewWorkspace_AllowsStandardCreationWhenOnlyInvitedStandardExists()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var owner = new ApplicationUser
        {
            Id = "owner-1",
            UserName = "owner@example.com",
            Email = "owner@example.com",
            EmailConfirmed = true
        };
        var currentUser = new ApplicationUser
        {
            Id = "user-1",
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            SubscriptionLevel = SubscriptionLevel.Standard
        };
        var invitedStandard = new Workspace
        {
            Name = "Owner Standard",
            Slug = "owner-standard",
            IsExplorer = false,
            OwnerUserId = owner.Id
        };

        dbContext.Users.AddRange(owner, currentUser);
        dbContext.Workspaces.Add(invitedStandard);
        await dbContext.SaveChangesAsync();

        dbContext.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            UserId = currentUser.Id,
            WorkspaceId = invitedStandard.Id
        });
        await dbContext.SaveChangesAsync();

        var controller = new WorkspaceSelectorController(
            new StubKeyValueService(),
            dbContext,
            new ConfigurationBuilder().Build(),
            new StubUserManager(currentUser))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, currentUser.Id)], "test"))
                }
            }
        };

        var result = await controller.SaveNewWorkspace(new CreateWorkspace
        {
            WorkspaceName = "My Standard",
            WorkspacePlan = WorkspacePlan.Standard
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("WorkspaceSelector", redirect.ControllerName);
        Assert.Equal(1, dbContext.Workspaces.Count(w => w.OwnerUserId == currentUser.Id && !w.IsExplorer));
    }
}
