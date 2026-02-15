using System.Security.Claims;
using FastGooey.Controllers;
using FastGooey.Models;
using FastGooey.Models.Media;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AccountManagementControllerTests
{
    private class StubKeyValueService : IKeyValueService
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

    private sealed class StubAuthenticationService : IAuthenticationService
    {
        public bool SignOutCalled { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignOutCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class StubTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class StubTempDataDictionaryFactory : ITempDataDictionaryFactory
    {
        public ITempDataDictionary GetTempData(HttpContext context)
            => new TempDataDictionary(context, new StubTempDataProvider());
    }

    [Fact]
    public async Task DeleteAccount_RemovesAccountAndWorkspaceData_AndSignsOut()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var workspace = new Workspace { Name = "Workspace", Slug = "workspace" };
        var user = new ApplicationUser { Id = "user-1", UserName = "user@example.com", Email = "user@example.com", Workspace = workspace };
        dbContext.Users.Add(user);
        dbContext.PasskeyCredentials.Add(new PasskeyCredential { UserId = user.Id, DescriptorId = [1], PublicKey = [2], CredentialType = "public-key" });
        dbContext.MagicLinkTokens.Add(new MagicLinkToken { UserId = user.Id, TokenHash = [3], ExpiresAt = clock.CurrentInstant.Plus(Duration.FromHours(1)) });
        dbContext.GooeyInterfaces.Add(new GooeyInterface { Workspace = workspace, Name = "Interface", Platform = "Mac" });
        dbContext.MediaSources.Add(new MediaSource { Workspace = workspace, Name = "Media", SourceType = MediaSourceType.S3 });
        dbContext.KeyValueStores.Add(new KeyValueStore { Key = $"account:{user.Id}", Value = "value" });
        await dbContext.SaveChangesAsync();

        var authService = new StubAuthenticationService();
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authService)
            .AddSingleton<ITempDataDictionaryFactory, StubTempDataDictionaryFactory>()
            .BuildServiceProvider();

        var controller = new AccountManagementController(new StubKeyValueService(), new StubUserManager(user), dbContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };

        var result = await controller.DeleteAccount();

        Assert.IsType<OkResult>(result);
        Assert.True(authService.SignOutCalled);
        Assert.Equal("/Home/Index", controller.Response.Headers["HX-Redirect"].ToString());
        Assert.Empty(dbContext.Users);
        Assert.Empty(dbContext.PasskeyCredentials);
        Assert.Empty(dbContext.MagicLinkTokens);
        Assert.Empty(dbContext.GooeyInterfaces);
        Assert.Empty(dbContext.MediaSources);
        Assert.Empty(dbContext.KeyValueStores);
        Assert.Empty(dbContext.Workspaces);
    }
}
