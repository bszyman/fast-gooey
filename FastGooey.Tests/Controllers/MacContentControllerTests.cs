using System.Text.Json;
using FastGooey.Controllers.Interfaces;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class MacContentControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
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
}