using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels.WorkspaceSelector;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace FastGooey.Controllers;

[Authorize]
[Route("Workspaces")]
public class WorkspaceSelectorController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }
        
        var workspaces = dbContext.Workspaces
            .Where(x => x.Users.Contains(currentUser))
            .ToList();

        var viewModel = new WorkspaceSelectorViewModel
        {
            Workspaces = workspaces,
            UserIsConfirmed = currentUser.EmailConfirmed
        };
        
        return View(viewModel);
    }

    [HttpGet("create")]
    public IActionResult CreateWorkspace(Guid workspaceId)
    {
        return View(new CreateWorkspace());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNewWorkspace(Guid workspaceId, CreateWorkspace form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (!currentUser.EmailConfirmed)
        {
            return RedirectToAction("Index", "WorkspaceSelector");
        }

        var helper = new SlugHelper();
        var slug = helper.GenerateSlug(form.WorkspaceName);
        
        var existingSlug = await dbContext.Workspaces.FirstOrDefaultAsync(w => w.Slug == slug);
        if (existingSlug is not null)
        {
            // Handle duplicate (e.g., append a number or return an error)
            ModelState.AddModelError("WorkspaceName", "A workspace with a similar name already exists.");
            return View(form);
        }
        
        var workspace = new Workspace
        {
            Name = form.WorkspaceName,
            Slug = slug
        };
        
        workspace.Users.Add(currentUser);

        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();
        
        return Redirect("/");
        
        // return RedirectToAction(
        //     "Home",
        //     "Workspaces",
        //     new { id = workspace.PublicId }
        // );
    }
}