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
        return View("~/Views/Interfaces/Workspaces/iOS/Content.cshtml");
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