using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class AccountManagementController(IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AccountWorkspace()
    {
        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml");
    }
    
    public IActionResult OrganizationWorkspace()
    {
        return PartialView("~/Views/AccountManagement/Workspaces/WorkspaceManagement.cshtml");
    }
}