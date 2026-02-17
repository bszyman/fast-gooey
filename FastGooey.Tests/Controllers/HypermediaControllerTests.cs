using System.Reflection;
using System.Text.Json;
using FastGooey.Controllers;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Services;
using FastGooey.Tests.Support;
using FastGooey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class HypermediaControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public async Task Get_ReturnsWidgetResponse_WhenPlatformIsWidget()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "Widget",
            ViewType = "Map",
            Name = "Map Widget",
            Config = JsonSerializer.SerializeToDocument(new MapJsonDataModel())
        };
        dbContext.GooeyInterfaces.Add(gooeyInterface);
        await dbContext.SaveChangesAsync();

        var controller = new HypermediaController(
            dbContext,
            memoryCache,
            new StubKeyValueService(),
            NullLogger<HypermediaController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Scheme = "https";
        controller.HttpContext.Request.Host = new HostString("example.com");

        var result = await controller.Get(gooeyInterface.DocId.ToBase64Url());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<WidgetMapHypermediaResponse>(ok.Value);
    }

    [Fact]
    public async Task GenerateWidgetResponse_ReturnsClockResponse()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var controller = new HypermediaController(
            dbContext,
            memoryCache,
            new StubKeyValueService(),
            NullLogger<HypermediaController>.Instance);

        var gooeyInterface = new GooeyInterface
        {
            Workspace = new Workspace { Name = "Test", Slug = "test" },
            Platform = "Widget",
            ViewType = "Clock",
            Name = "Clock Widget",
            Config = JsonSerializer.SerializeToDocument(new ClockJsonDataModel
            {
                Location = "New York",
                Latitude = "40.7128",
                Longitude = "-74.0060"
            })
        };

        var response = await InvokeGenerateWidgetResponse(controller, gooeyInterface);

        var clockResponse = Assert.IsType<WidgetClockHypermediaResponse>(response);
        Assert.Equal("New York", clockResponse.Location);
        Assert.False(string.IsNullOrWhiteSpace(clockResponse.Time));
    }

    [Fact]
    public async Task GenerateWidgetResponse_ReturnsMapPins()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var controller = new HypermediaController(
            dbContext,
            memoryCache,
            new StubKeyValueService(),
            NullLogger<HypermediaController>.Instance);

        var pinId = Guid.NewGuid();
        var gooeyInterface = new GooeyInterface
        {
            Workspace = new Workspace { Name = "Test", Slug = "test" },
            Platform = "Widget",
            ViewType = "Map",
            Name = "Map Widget",
            Config = JsonSerializer.SerializeToDocument(new MapJsonDataModel
            {
                Pins =
                [
                    new MapWorkspacePinModel
                    {
                        EntryId = pinId,
                        Latitude = "40.0",
                        Longitude = "-70.0",
                        LocationName = "Pin"
                    }
                ]
            })
        };

        var response = await InvokeGenerateWidgetResponse(controller, gooeyInterface);

        var mapResponse = Assert.IsType<WidgetMapHypermediaResponse>(response);
        var pin = Assert.Single(mapResponse.Pins);
        Assert.Equal(pinId, pin.EntryId);
    }

    private static async Task<IHypermediaResponse> InvokeGenerateWidgetResponse(
        HypermediaController controller,
        GooeyInterface gooeyInterface)
    {
        var method = typeof(HypermediaController).GetMethod("GenerateWidgetResponse", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = Assert.IsType<Task<IHypermediaResponse>>(method.Invoke(controller, [gooeyInterface]));
        return await task;
    }
}
