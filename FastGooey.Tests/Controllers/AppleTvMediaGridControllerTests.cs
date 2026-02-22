using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.AppleTv.MediaGrid.Controllers;
using FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using FastGooey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AppleTvMediaGridControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public void MediaGridItemPanelForm_RequiresTitleLinkAndPreviewMedia()
    {
        var form = new MediaGridItemPanelFormModel();
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, x => x.MemberNames.Contains("Title"));
        Assert.Contains(results, x => x.MemberNames.Contains("LinkTo"));
        Assert.Contains(results, x => x.MemberNames.Contains("PreviewMedia"));
    }

    [Fact]
    public async Task SaveMediaGridItemPanel_ReturnsRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var contentNode = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "MediaGrid",
            Config = JsonSerializer.SerializeToDocument(new AppleTvMediaGridJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvMediaGridController(
            NullLogger<AppleTvMediaGridController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Title", "Required");

        var result = await controller.SaveMediaGridItemPanel(
            contentNode.DocId.ToBase64Url(),
            null,
            new MediaGridItemPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/MediaGridItemPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public async Task SaveWorkspace_PersistsTrimmedValues()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var contentNode = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "MediaGrid",
            Config = JsonSerializer.SerializeToDocument(new AppleTvMediaGridJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvMediaGridController(
            NullLogger<AppleTvMediaGridController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        await controller.SaveWorkspace(
            contentNode.DocId.ToBase64Url(),
            new MediaWorkspaceFormModel
            {
                Title = "  Grid Title  "
            });

        var saved = dbContext.GooeyInterfaces.Single(x => x.Id == contentNode.Id)
            .Config.Deserialize<AppleTvMediaGridJsonDataModel>();

        Assert.NotNull(saved);
        Assert.Equal("Grid Title", saved!.Title);
    }
}
