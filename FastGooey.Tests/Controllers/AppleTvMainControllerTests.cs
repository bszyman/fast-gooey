using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.AppleTv.Main.Controllers;
using FastGooey.Features.Interfaces.AppleTv.Main.Models;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using FastGooey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AppleTvMainControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public void BackgroundForm_RequiresImageResource()
    {
        var form = new AppleTvMainBackgroundEditorPanelFormModel { ImageResource = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, x => x.MemberNames.Contains("ImageResource"));
    }

    [Fact]
    public async Task SaveBackgroundEditorPanel_ReturnsRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var contentNode = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Main",
            Config = JsonSerializer.SerializeToDocument(new MainJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvMainController(
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("ImageResource", "Required");

        var result = await controller.SaveBackgroundEditorPanel(
            contentNode.DocId.ToBase64Url(),
            new AppleTvMainBackgroundEditorPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/backgroundEditorPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }
}
