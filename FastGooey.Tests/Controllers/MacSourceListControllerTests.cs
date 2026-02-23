using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.Mac.SourceList.Controllers;
using FastGooey.Features.Interfaces.Mac.SourceList.Models;
using FastGooey.Models;
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
        await using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
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
        Assert.Equal("Partials/SourceListEditorPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }
    
    [Fact]
    public async Task SaveItemEditorPanel_ReturnsItemEditorPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        await using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace
        {
            Name = "Test Workspace",
            Slug = "test-workspace"
        };
        await dbContext.Workspaces.AddAsync(workspace);
        await dbContext.SaveChangesAsync();

        var interfaceId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var data = new MacSourceListJsonDataModel
        {
            Groups =
            [
                new MacSourceListGroupJsonDataModel
                {
                    Identifier = groupId
                }
            ]
        };

        await dbContext.GooeyInterfaces.AddAsync(new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Platform = "Mac",
            ViewType = "SourceList",
            Name = "Test Source List",
            DocId = interfaceId,
            Config = JsonSerializer.SerializeToDocument(data)
        });
        await dbContext.SaveChangesAsync();

        var controller = new MacSourceListController(
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Title", "Required");

        var result = await controller.SaveItemEditorPanel(interfaceId.ToString(), groupId, null, new MacSourceListGroupItemPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/SourceListItemEditorPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public void MacSourceListGroupItemPanelFormModel_RequiresTitle()
    {
        var form = new MacSourceListGroupItemPanelFormModel { Title = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }
}
