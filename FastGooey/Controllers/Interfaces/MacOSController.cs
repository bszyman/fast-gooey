using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Route("Interfaces/MacOS")]
public class MacOSController(
    ILogger<InterfacesController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    [HttpGet("InterfaceSelectorPanel")]
    public IActionResult MacOSInterfaceSelectorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/MacOSInterfaceSelectorPanel.cshtml");
    }
}