using System.Text.Json;
using FastGooey.Controllers.Interfaces;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.Common;
using FastGooey.Models.JsonDataModels;
using FastGooey.Tests.Support;
using FastGooey.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class ContentInterfaceControllerBaseTests
{
    private class TestContentDataModel : IContentDataModel<TestContentItem>
    {
        public string HeaderTitle { get; set; } = string.Empty;
        public string HeaderBackgroundImage { get; set; } = string.Empty;
        public List<TestContentItem> Items { get; set; } = new();
    }

    private class TestContentItem : ContentItemBase
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestWorkspaceViewModel : ContentWorkspaceViewModelBase<TestContentDataModel> { }
    private class TestWorkspaceFormModel : ContentWorkspaceFormModelBase { }

    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    private class ConcreteContentController(IKeyValueService keyValueService, ApplicationDbContext dbContext) 
        : ContentInterfaceControllerBase<TestContentDataModel, TestContentItem, TestWorkspaceViewModel, TestWorkspaceFormModel>(keyValueService, dbContext)
    {
        protected override string Platform => "TestPlatform";
        protected override string ViewType => "TestView";
        protected override string BaseViewPath => "~/Views/Test";

        public async Task<IActionResult> SaveItem(Guid interfaceId, Guid? itemId, string value)
        {
            return await SaveContentItemInternal<TestContentItem, string>(
                interfaceId,
                itemId,
                value,
                "test",
                (item, val) => item.Value = val
            );
        }
    }

    [Fact]
    public async Task SaveContentItemInternal_AddsNewItem_WhenItemIdIsNull()
    {
        // Arrange
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var keyValueService = new StubKeyValueService();
        
        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Interface",
            Platform = "TestPlatform",
            Config = JsonSerializer.SerializeToDocument(new TestContentDataModel())
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new ConcreteContentController(keyValueService, dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.SaveItem(gooeyInterface.DocId, null, "New Value");

        // Assert
        var updatedInterface = await dbContext.GooeyInterfaces.FirstAsync(x => x.Id == gooeyInterface.Id);
        var data = JsonSerializer.Deserialize<TestContentDataModel>(updatedInterface.Config);
        Assert.Single(data.Items);
        Assert.Equal("New Value", data.Items[0].Value);
        Assert.IsType<PartialViewResult>(result);
    }
}
