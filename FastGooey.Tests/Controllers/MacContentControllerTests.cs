using System.Text.Json;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using FastGooey.Features.Interfaces.Mac.Content.Controllers;
using FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels.Mac;
using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;

namespace FastGooey.Tests.Controllers;

public class MacContentControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }
    
    [Fact]
    public async Task SaveHeadline_ReturnsHeadlinePanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        var controller = new MacContentController(
            NullLogger<MacContentController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Headline", "Required");

        var result = await controller.SaveHeadline(Guid.NewGuid(), Guid.NewGuid().ToString(), null, new HeadlineContentFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/ContentHeadlineConfigurationPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"]);
    }

    [Fact]
    public void HeadlineContentFormModel_RequiresHeadline()
    {
        var form = new HeadlineContentFormModel { Headline = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Headline"));
    }

    [Fact]
    public async Task SaveLink_ReturnsEditorPanel_WhenModelStateIsInvalid()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var controller = new MacContentController(
            NullLogger<MacContentController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Title", "Title is required");

        var result = await controller.SaveLink(Guid.NewGuid(), Guid.NewGuid().ToString(), null, new LinkContentFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/ContentLinkConfigurationPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public async Task DeleteItem_RemovesItem_WhenExists()
    {
        // Arrange
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var keyValueService = new StubKeyValueService();
        var logger = NullLogger<MacContentController>.Instance;

        var itemId = Guid.NewGuid();
        var dataModel = new MacContentJsonDataModel  // Use Mac-specific model for realism
        {
            Items = new List<MacContentItemJsonDataModel> { new MacContentItemJsonDataModel { Identifier = itemId, ContentType = "test" } }  // Adapt to Mac types
        };

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Interface",
            Platform = "Mac",  // Match the controller's platform
            Config = JsonSerializer.SerializeToDocument(dataModel)
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new MacContentController(logger, keyValueService, dbContext);  // Use MacContentController
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act - Call the public DeleteItem method (simulates the HTTP DELETE request)
        var result = await controller.DeleteItem(workspace.PublicId, gooeyInterface.DocId.ToString(), itemId);

        // Assert - (Unchanged, but adapt to MacContentJsonDataModel if needed)
        var updatedInterface = await dbContext.GooeyInterfaces.FirstAsync(x => x.Id == gooeyInterface.Id);
        var data = JsonSerializer.Deserialize<MacContentJsonDataModel>(updatedInterface.Config);
        Assert.Empty(data.Items);
        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task SaveText_ReturnsEditorPanel_WhenModelStateIsInvalid()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var controller = new MacContentController(
            NullLogger<MacContentController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Text", "The Text field is required.");

        var result = await controller.SaveText(Guid.NewGuid(), Guid.NewGuid().ToString(), null, new TextContentFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/ContentTextConfigurationPanel", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }

    [Fact]
    public void TextContentFormModel_RequiresText()
    {
        var form = new TextContentFormModel { Text = string.Empty };
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Text"));
    }
}