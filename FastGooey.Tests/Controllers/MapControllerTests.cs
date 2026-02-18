using System.ComponentModel.DataAnnotations;
using FastGooey.Features.Widgets.Map.Controllers;
using FastGooey.Features.Widgets.Map.Models.FormModels;
using FastGooey.Features.Widgets.Map.Models.ViewModels.Map;
using FastGooey.Features.Widgets.Weather.Controllers;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class MapControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }
    
    [Fact]
    public void AddLocationEntry_ReturnsSearchPanelWithRetargetHeader_WhenModelStateIsInvalid()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var controller = new MapController(
            NullLogger<WeatherController>.Instance,
            new StubKeyValueService(),
            dbContext,
            null!);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError(nameof(MapAddLocationEntryFormModel.LocationName), "Required");

        var result = controller.AddLocationEntry(new MapAddLocationEntryFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("Partials/SearchPanel", partial.ViewName);
        Assert.IsType<MapSearchPanelViewModel>(partial.Model);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }
    
    [Fact]
    public void MapAddLocationEntryFormModel_RequiresAllFields()
    {
        var form = new MapAddLocationEntryFormModel();
        var context = new ValidationContext(form);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(form, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MapAddLocationEntryFormModel.LocationName)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MapAddLocationEntryFormModel.LocationIdentifier)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MapAddLocationEntryFormModel.Latitude)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MapAddLocationEntryFormModel.Longitude)));
    }
}
