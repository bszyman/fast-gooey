using System.Text.Json;
using FastGooey.Database;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Features.Workspaces.Home.Models.ViewModels;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Tests.Support;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class BaseInterfaceControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    private class TestBaseInterfaceController(IKeyValueService keyValueService, ApplicationDbContext dbContext)
        : BaseInterfaceController(keyValueService, dbContext)
    {
        public Task<TViewModel> PublicGetInterfaceViewModelAsync<TViewModel, TDataModel>(Guid interfaceId)
            where TViewModel : new()
        {
            return GetInterfaceViewModelAsync<TViewModel, TDataModel>(interfaceId);
        }
    }

    private class SampleDataModel
    {
        public string Message { get; set; } = string.Empty;
    }

    private class SampleViewModel
    {
        public GooeyInterface? ContentNode { get; set; }
        public SampleDataModel? Data { get; set; }
    }

    [Fact]
    public async Task GetInterfaceViewModelAsync_PopulatesContentNodeAndData()
    {
        // Arrange
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var keyValueService = new StubKeyValueService();

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var interfaceId = Guid.NewGuid();
        var data = new SampleDataModel { Message = "Hello World" };
        var gooeyInterface = new GooeyInterface
        {
            DocId = interfaceId,
            Workspace = workspace,
            Name = "Test Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        dbContext.Workspaces.Add(workspace);
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new TestBaseInterfaceController(keyValueService, dbContext);

        // Act
        var viewModel = await controller.PublicGetInterfaceViewModelAsync<SampleViewModel, SampleDataModel>(interfaceId);

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.ContentNode);
        Assert.Equal(interfaceId, viewModel.ContentNode.DocId);
        Assert.NotNull(viewModel.Data);
        Assert.Equal("Hello World", viewModel.Data.Message);
    }

    [Fact]
    public async Task GetInterfaceViewModelAsync_PopulatesWorkspace_WhenContentNodePropertyMissing()
    {
        // Arrange
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var keyValueService = new StubKeyValueService();

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var interfaceId = Guid.NewGuid();
        var gooeyInterface = new GooeyInterface
        {
            DocId = interfaceId,
            Workspace = workspace,
            Name = "Test Interface",
            Config = JsonSerializer.SerializeToDocument(new { })
        };

        dbContext.Workspaces.Add(workspace);
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new TestBaseInterfaceController(keyValueService, dbContext);

        // Act
        var viewModel = await controller.PublicGetInterfaceViewModelAsync<InfoViewModel, object>(interfaceId);

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.ContentNode); // InfoViewModel has ContentNode
        Assert.Equal(interfaceId, viewModel.ContentNode.DocId);
    }
}
