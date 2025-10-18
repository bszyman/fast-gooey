using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Route("Interfaces/MacOS")]
public class MacOSController(
    ILogger<MacOSController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    [HttpGet("InterfaceSelectorPanel")]
    public IActionResult MacOSInterfaceSelectorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/MacOSInterfaceSelectorPanel.cshtml");
    }
    
    [HttpGet("Table")]
    public IActionResult Table()
    {
        return View();
    }
    
    [HttpGet("TableWorkspace")]
    public IActionResult TableWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Table.cshtml");
    }
    
    [HttpGet("TableStructureWorkspace")]
    public IActionResult TableStructureWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/TableStructure.cshtml");
    }

    [HttpGet("TableHeaderOptionRow")]
    public IActionResult TableHeaderOptionRow()
    {
        return PartialView("~/Views/MacOS/Partials/TableHeaderOptionRow.cshtml");
    }
    
    [HttpGet("TableFieldEditorPanel")]
    public IActionResult TableFieldEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/TableFieldEditorPanel.cshtml");
    }

    [HttpGet("TableItemEditorPanel")]
    public IActionResult TableItemEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/TableItemEditorPanel.cshtml");    
    }
    
    [HttpGet("SourceList")]
    public IActionResult SourceList()
    {
        return View();
    }
    
    [HttpGet("SourceListWorkspace")]
    public IActionResult SourceListWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/SourceList.cshtml");
    }
    
    [HttpGet("Outline")]
    public IActionResult Outline()
    {
        return View();
    }
    
    [HttpGet("OutlineWorkspace")]
    public IActionResult OutlineWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Outline.cshtml");
    }
    
    [HttpGet("Form")]
    public IActionResult Form()
    {
        return View();
    }
    
    [HttpGet("FormWorkspace")]
    public IActionResult FormWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Form.cshtml");
    }
    
    [HttpGet("Content")]
    public IActionResult Content()
    {
        return View();
    }
    
    [HttpGet("ContentWorkspace")]
    public IActionResult ContentWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Content.cshtml");
    }
}