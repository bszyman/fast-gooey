using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.Mac.Collection.Controllers;
using FastGooey.Features.Interfaces.Mac.Collection.Models;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class MacCollectionControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public async Task CollectionViewItemEditorPanelWithItem_ReturnsEditorPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Interface",
            Platform = "Mac",
            ViewType = "Collection",
            Config = JsonSerializer.SerializeToDocument(new MacCollectionViewJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new MacCollectionController(
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Title", "Required");

        var form = new MacCollectionEditorPanelFormModel
        {
            Title = string.Empty,
            ImageUrl = "image",
            Url = "url"
        };

        var result = await controller.CollectionViewItemEditorPanelWithItem(
            gooeyInterface.DocId.ToString(),
            null,
            form);

        var partial = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<MacInterfaceCollectionEditorPanelViewModel>(partial.Model);
        Assert.Equal("Partials/CollectionViewItemEditorPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
        Assert.Equal(form.ImageUrl, model.Item.ImageUrl);
        Assert.Equal(form.Url, model.Item.Url);
    }

    [Fact]
    public void MacCollectionEditorPanelFormModel_RequiresTitle()
    {
        var form = new MacCollectionEditorPanelFormModel { Title = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }
}
