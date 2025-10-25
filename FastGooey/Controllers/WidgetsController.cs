using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

[Route("Workspaces/{workspaceId:guid}/Widgets")]
public class WidgetsController(
    ILogger<WidgetsController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
    ): BaseStudioController(keyValueService, dbContext)
{
    [HttpGet("NewWidgetPanel")]
    public IActionResult NewWidgetPartialView()
    {
        return PartialView("~/Views/Widgets/Partials/NewWidgetPartialView.cshtml", WorkspaceId);
    }
}