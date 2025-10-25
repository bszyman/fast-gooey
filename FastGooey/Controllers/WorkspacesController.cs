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
[Route("Workspaces/{workspaceId:guid}")]
public class WorkspacesController(
    ILogger<WorkspacesController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet("Home")]
    public IActionResult Home(Guid workspaceId)
    {
        return View();
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
            nameof(Home), 
            new { id = workspace.PublicId }
        );
    }

    [HttpGet("Info/{interfaceId:guid}")]
    public IActionResult Info(Guid workspaceId, Guid interfaceId)
    {
        var interfaceNode = dbContext.GooeyInterfaces
            .First(x => x.DocId.Equals(interfaceId));
        
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == workspaceId);

        var viewModel = new InfoViewModel
        {
            ContentNode = interfaceNode,
            Workspace = workspace
        };
        
        return View(viewModel);
    }

    [HttpPost("Edit")]
    public IActionResult EditWorkspace(Guid workspaceId)
    {
        return View();
    }

    [HttpDelete("Delete")]
    public IActionResult DeleteWorkspace(Guid workspaceId)
    {
        return View();
    }
    
    [HttpPost("UpdateTitle/{interfaceId:guid}")]
    public async Task<IActionResult> UpdateTitle(Guid interfaceId, [FromForm] string title)
    {
        var interfaceNode = dbContext.GooeyInterfaces
            .First(x => x.DocId.Equals(interfaceId));
        
        interfaceNode.Name = title;
        await dbContext.SaveChangesAsync();
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return Ok();
    }
}