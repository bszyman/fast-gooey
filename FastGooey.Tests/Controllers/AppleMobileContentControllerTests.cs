using FastGooey.Controllers.Interfaces;
using FastGooey.Models.FormModels;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;

namespace FastGooey.Tests.Controllers;

public class AppleMobileContentControllerTests
{
    private class StubKeyValueService : IKeyValueService
    {
        public Task<string?> GetValueForKey(string key) => Task.FromResult<string?>(null);
        public Task SetValueForKey(string key, string value) => Task.CompletedTask;
    }

    [Fact]
    public async Task SaveText_ReturnsEditorPanel_WhenModelStateIsInvalid()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var dbContext = TestDbContextFactory.Create(clock);
        var controller = new AppleMobileContentController(
            NullLogger<AppleMobileContentController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Text", "The Text field is required.");

        var result = await controller.SaveText(Guid.NewGuid(), Guid.NewGuid().ToString(), null, new TextContentFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/AppleMobileContent/Partials/ContentTextConfigurationPanel.cshtml", partial.ViewName);
        Assert.Equal("#editorPanel", controller.Response.Headers["HX-Retarget"].ToString());
    }
}
