using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class AccountManagementController(
    IKeyValueService keyValueService,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }
        
        var viewModel = CreateViewModel(currentUser);
        
        return View(viewModel);
    }

    [HttpGet("Workspace")]
    public async Task<IActionResult> AccountWorkspace()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }
        
        var viewModel = CreateViewModel(currentUser);
        
        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
    }

    [HttpPost("Workspace/Save")]
    public async Task<IActionResult> SaveAccountWorkspace([Bind(Prefix = "FormModel")] AccountManagementFormModel formModel)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }
        
        currentUser.FirstName = formModel.FirstName;
        currentUser.LastName = formModel.LastName;
        
        var result = await userManager.UpdateAsync(currentUser);
    
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        var viewModel = CreateViewModel(currentUser);
        
        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
    }
    
    private ManageAccountViewModel CreateViewModel(ApplicationUser user)
    {
        return new ManageAccountViewModel
        {
            User = user,
            FormModel = new AccountManagementFormModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
            }
        };
    }
}