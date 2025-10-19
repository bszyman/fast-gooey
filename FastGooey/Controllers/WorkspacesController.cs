using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class WorkspacesController(ILogger<WorkspacesController> logger, IKeyValueService keyValueService): BaseStudioController(keyValueService)
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Info()
    {
        return View();
    }
}