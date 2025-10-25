using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

[Authorize]
[Route("Workspaces")]
public class WorkspaceSelectorController(
    ILogger<WorkspacesController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager): 
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
        
        var workspaces = dbContext.Workspaces
            .Where(x => x.Users.Contains(currentUser))
            .ToList();
        
        return View(workspaces);
    }

    [HttpGet("CreateWorkspace")]
    public IActionResult CreateWorkspace(Guid workspaceId)
    {
        return View(new CreateWorkspace());
    }

    [HttpPost("CreateWorkspace")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkspace(Guid workspaceId, CreateWorkspace form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var workspace = new Workspace
        {
            Name = form.WorkspaceName
        };
        
        workspace.Users.Add(currentUser);

        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();
        
        return RedirectToAction(
            "Home",
            "Workspaces",
            new { id = workspace.PublicId }
        );
    }
}