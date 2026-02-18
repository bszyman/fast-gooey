using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels;
using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;
using FastGooey.Features.Interfaces.Mac.Table.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class MacTableControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public async Task SaveTableFieldEditorPanel_ReturnsEditorPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Interface",
            Platform = "Mac",
            ViewType = "Table",
            Config = JsonSerializer.SerializeToDocument(new MacTableJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new MacTableController(
            NullLogger<MacTableController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("FieldName", "Required");

        var result = await controller.SaveTableFieldEditorPanel(gooeyInterface.DocId.ToString(), null, new MacTableFieldConfigPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/TableFieldEditorPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public void MacTableFieldConfigPanelFormModel_RequiresFields()
    {
        var form = new MacTableFieldConfigPanelFormModel
        {
            FieldName = string.Empty,
            FieldAlias = string.Empty,
            FieldType = string.Empty
        };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("FieldName"));
        Assert.Contains(results, r => r.MemberNames.Contains("FieldAlias"));
        Assert.Contains(results, r => r.MemberNames.Contains("FieldType"));
    }
}
