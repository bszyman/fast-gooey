using System.ComponentModel.DataAnnotations;
using FastGooey.Features.Widgets.Clock.Controllers;
using FastGooey.Features.Widgets.Clock.Models.FormModels;
using FastGooey.Features.Widgets.Weather.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class ClockControllerTests
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

    private sealed class StubUserManager : UserManager<ApplicationUser>
    {
        public StubUserManager() : base(
            new StubUserStore(),
            null!,
            new PasswordHasher<ApplicationUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance)
        {
        }
    }

    [Fact]
    public async Task SaveWorkspace_ReturnsSearchPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var controller = new ClockController(
            NullLogger<ClockController>.Instance,
            new StubKeyValueService(),
            dbContext,
            new StubUserManager());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("MapIdentifier", "Required");

        var result = await controller.SaveWorkspace(Guid.NewGuid().ToString(), new ClockFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/SearchPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public void ClockFormModel_RequiresMapIdentifier()
    {
        var form = new ClockFormModel { MapIdentifier = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("MapIdentifier"));
    }
}
