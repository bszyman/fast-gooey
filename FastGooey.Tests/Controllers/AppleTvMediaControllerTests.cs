using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.AppleTv.Media.Controllers;
using FastGooey.Features.Interfaces.AppleTv.Media.Models;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using FastGooey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AppleTvMediaControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public void WorkspaceForm_RequiresMediaUrl()
    {
        var form = new MediaWorkspaceFormModel { MediaUrl = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, x => x.MemberNames.Contains("MediaUrl"));
    }

    [Fact]
    public async Task SaveWorkspace_ReturnsRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var contentNode = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Media",
            Config = JsonSerializer.SerializeToDocument(new AppleTvMediaJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvMediaController(
            NullLogger<AppleTvMediaController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("MediaUrl", "Required");

        var result = await controller.SaveWorkspace(
            contentNode.DocId.ToBase64Url(),
            new MediaWorkspaceFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Workspace", partial.ViewName);
        Assert.Equal("#workspaceEditor", controller.Response.Headers["HX-Retarget"].ToString());
    }
}
