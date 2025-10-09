using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class StudioController(ILogger<StudioController> logger, IKeyValueService keyValueService): BaseStudioController(keyValueService)
{
    public IActionResult Index()
    {
        return View();
    }
}