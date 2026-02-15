using FastGooey.Controllers.Interfaces;
using FastGooey.Models.FormModels;
using FastGooey.Services;
using FastGooey.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using System.ComponentModel.DataAnnotations;

namespace FastGooey.Tests.Controllers;

public class AppleMobileContentControllerTests
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
        var controller = new AppleMobileContentController(
            NullLogger<AppleMobileContentController>.Instance,
            new StubKeyValueService(),
            dbContext);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ModelState.AddModelError("Headline", "Required");

        var result = await controller.SaveHeadline(Guid.NewGuid(), Guid.NewGuid().ToString(), null, new HeadlineContentFormModel());

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/AppleMobileContent/Partials/ContentHeadlineConfigurationPanel.cshtml", partial.ViewName);
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
