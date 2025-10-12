using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class InterfacesController(ILogger<InterfacesController> logger, IKeyValueService keyValueService): BaseStudioController(keyValueService)
{
    // iOS List
    public IActionResult AppleMobileList()
    {
        return View();
    }
    
    public IActionResult AppleMobileListWorkspace()
    {
        return View("~/Views/Interfaces/Workspaces/iOS/List.cshtml");
    }
    
    // iOS Content
    public IActionResult AppleMobileContent()
    {
        return View();
    }
    
    public IActionResult AppleMobileContentWorkspace()
    {
        return PartialView("~/Views/Interfaces/Workspaces/iOS/Content.cshtml");
    }

    public IActionResult AppleMobileContentTypeSelectorPanel()
    {
        return PartialView("~/Views/Interfaces/Partials/AppleMobileContentTypeSelectorPanel.cshtml");
    }

    // [HttpGet("Content/HeadlineConfigurationPanel")]
    public IActionResult ContentHeadlineConfigurationPanel()
    {
        return PartialView("~/Views/Interfaces/Partials/ContentHeadlineConfigurationPanel.cshtml");
    }
    
    [HttpGet("Content/TextConfigurationPanel")]
    public IActionResult ContentTextConfigurationPanel()
    {
        return PartialView("~/Views/Interfaces/Partials/ContentTextConfigurationPanel.cshtml");
    }
    
    [HttpGet("Content/ImageConfigurationPanel")]
    public IActionResult ContentImageConfigurationPanel()
    {
        return PartialView("~/Views/Interfaces/Partials/ContentImageConfigurationPanel.cshtml");
    }
    
    [HttpGet("Content/VideoConfigurationPanel")]
    public IActionResult ContentVideoConfigurationPanel()
    {
        return PartialView("~/Views/Interfaces/Partials/ContentVideoConfigurationPanel.cshtml");
    }
    
    // iOS Form
    public IActionResult AppleMobileForm()
    {
        return View();
    }
    
    public IActionResult AppleMobileFormWorkspace()
    {
        return View("~/Views/Interfaces/Workspaces/iOS/Form.cshtml");
    }
}