using System.Text.Json;
using FastGooey.Controllers.Interfaces;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class MacSourceListControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public async Task SaveGroupEditorPanel_ReturnsEditorPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var groupId = Guid.NewGuid();
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Interface",
            Platform = "Mac",
            ViewType = "SourceList",
            Config = JsonSerializer.SerializeToDocument(new MacSourceListJsonDataModel
            {
                Groups = [new MacSourceListGroupJsonDataModel { Identifier = groupId, GroupName = "Existing Group" }]
            })
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new MacSourceListController(
            NullLogger<MacSourceListController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("GroupName", "Required");

        var result = await controller.SaveGroupEditorPanel(
            gooeyInterface.DocId.ToString(),
            groupId,
            new MacSourceListGroupPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/MacSourceList/Partials/SourceListEditorPanel.cshtml", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }
}
