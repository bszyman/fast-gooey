using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FastGooey.Features.Interfaces.AppleTv.Product.Controllers;
using FastGooey.Features.Interfaces.AppleTv.Product.Models;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using FastGooey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AppleTvProductControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public void RelatedItemPanelForm_RequiresTitleLinkAndMediaUrl()
    {
        var form = new RelatedItemPanelFormModel();
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, x => x.MemberNames.Contains("Title"));
        Assert.Contains(results, x => x.MemberNames.Contains("Link"));
        Assert.Contains(results, x => x.MemberNames.Contains("MediaUrl"));
    }

    [Fact]
    public async Task SaveRelatedItemPanel_ReturnsRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var contentNode = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Product",
            Config = JsonSerializer.SerializeToDocument(new AppleTvProductJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvProductController(
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Title", "Required");

        var result = await controller.SaveRelatedItemPanel(
            contentNode.DocId.ToBase64Url(),
            null,
            new RelatedItemPanelFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/RelatedItemPanel", partial.ViewName);
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
            ViewType = "Product",
            Config = JsonSerializer.SerializeToDocument(new AppleTvProductJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(contentNode);
        await dbContext.SaveChangesAsync();

        var controller = new AppleTvProductController(
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        await controller.SaveWorkspace(
            contentNode.DocId.ToBase64Url(),
            new ProductWorkspaceFormModel
            {
                Title = "  Product Title  ",
                Description = "  Product Description  ",
                PreviewMediaUrl = "  https://example.com/preview.png  "
            });

        var saved = dbContext.GooeyInterfaces.Single(x => x.Id == contentNode.Id)
            .Config.Deserialize<AppleTvProductJsonDataModel>();

        Assert.NotNull(saved);
        Assert.Equal("Product Title", saved!.Title);
        Assert.Equal("Product Description", saved.Description);
        Assert.Equal("https://example.com/preview.png", saved.PreviewMediaUrl);
    }
}
