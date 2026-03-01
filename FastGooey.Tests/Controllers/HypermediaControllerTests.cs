using System.Reflection;
using System.Text.Json;
using FastGooey.Controllers;
using FastGooey.Features.Interfaces.AppleTv.Main.Models;
using FastGooey.Features.Interfaces.AppleTv.Media.Models;
using FastGooey.Features.Interfaces.AppleTv.Detail.Models;
using FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;
using FastGooey.Features.Widgets.Clock.Models.JsonDataModels;
using FastGooey.Features.Widgets.Map.Models.JsonDataModels;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
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
    public async Task Get_ReturnsAppleTvMainResponse_WhenPlatformIsAppleTvMain()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Main",
            Name = "AppleTv Main",
            Config = JsonSerializer.SerializeToDocument(new MainJsonDataModel
            {
                BackgroundSplash = new BackgroundSplash
                {
                    ImageResource = "https://example.com/background.png",
                    AudioResource = "https://example.com/audio.mp3"
                },
                MenuBarButtons =
                [
                    new NavigationButtonJsonDataModel
                    {
                        Text = "Home",
                        Link = "https://example.com/home"
                    }
                ]
            })
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
        var response = Assert.IsType<AppleTvMainHypermediaResponse>(ok.Value);
        Assert.Equal("AppleTv", response.Platform);
        Assert.Equal("Main", response.View);
    }

    [Fact]
    public async Task Get_ReturnsAppleTvMediaResponse_WhenPlatformIsAppleTvMedia()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            DocId = Guid.NewGuid(),
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Media",
            Name = "AppleTv Media",
            Config = JsonSerializer.SerializeToDocument(new AppleTvMediaJsonDataModel())
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
        var response = Assert.IsType<AppleTvMediaHypermediaResponse>(ok.Value);
        Assert.Equal("AppleTv", response.Platform);
        Assert.Equal("Media", response.View);
        Assert.Equal(string.Empty, response.Content.MediaUrl);
    }
  
      [Fact]
    public async Task Get_ReturnsAppleTvDetailResponse_WhenPlatformIsAppleTvProduct()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Detail",
            Name = "AppleTv Detail",
            Config = JsonSerializer.SerializeToDocument(new AppleTvDetailJsonDataModel
            {
                Title = "Featured",
                Description = "A featured product",
                PreviewMediaUrl = "https://example.com/preview.png",
                RelatedItems =
                [
                    new AppleTvDetailRelatedItemJsonModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Related",
                        Link = "https://example.com/related",
                        MediaUrl = "https://example.com/related.png"
                    }
                ]
            })
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
        var response = Assert.IsType<AppleTvDetailHypermediaResponse>(ok.Value);
        Assert.Equal("AppleTv", response.Platform);
        Assert.Equal("Detail", response.View);
        Assert.Equal(gooeyInterface.DocId, response.InterfaceId);
        Assert.NotNull(response.Content.RelatedItems);
    }

    [Fact]
    public async Task Get_ReturnsAppleTvMediaGridResponse_WhenPlatformIsAppleTvMediaGrid()
    {
        using var dbContext = TestDbContextFactory.Create(new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0)));
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var workspace = new Workspace { Name = "Test", Slug = "test" };
        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "MediaGrid",
            Name = "AppleTv Media Grid",
            Config = JsonSerializer.SerializeToDocument(new AppleTvMediaGridJsonDataModel
            {
                Title = "Featured Grid",
                MediaItems =
                [
                    new AppleTvMediaGridItemJsonDataModel
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Title = "Item 1",
                        LinkTo = "https://example.com/item-1",
                        PreviewMedia = "https://example.com/item-1.png"
                    }
                ]
            })
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
        var response = Assert.IsType<AppleTvMediaGridHypermediaResponse>(ok.Value);
        Assert.Equal("AppleTv", response.Platform);
        Assert.Equal("MediaGrid", response.View);
        Assert.Equal(gooeyInterface.DocId, response.InterfaceId);
        Assert.NotNull(response.Content.MediaItems);
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
