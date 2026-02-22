using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Workspaces.Selector.Models.FormModels;
using FastGooey.Features.Workspaces.Selector.Models;
using FastGooey.Features.Workspaces.Selector.Models.ViewModels.WorkspaceSelector;
using FastGooey.Models;
using FastGooey.Models.Configuration;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace FastGooey.Features.Workspaces.Selector.Controllers;

[Authorize]
[Route("Workspaces")]
public class WorkspaceSelectorController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager) :
    BaseStudioController(keyValueService, dbContext)
{
    private readonly StripeConfigurationModel? _stripeConfig = configuration.GetSection("Stripe").Get<StripeConfigurationModel>();

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (currentUser.PasskeyRequired)
        {
            var hasPasskey = await dbContext.PasskeyCredentials
                .AnyAsync(p => p.UserId == currentUser.Id);
            if (!hasPasskey)
            {
                return RedirectToAction(
                    "Complete",
                    "SignUp",
                    new { returnUrl = Url.Action("Index", "WorkspaceSelector") }
                );
            }
        }

        var workspaces = await dbContext.Workspaces
            .Where(x => x.OwnerUserId == currentUser.Id || x.Users.Any(u => u.Id == currentUser.Id))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        
        var explorerWorkspace = workspaces.FirstOrDefault(w => w.IsExplorer);
        var standardWorkspaces = workspaces.Where(w => !w.IsExplorer).ToList();
        var standardWorkspaceAllowance = GetStandardWorkspaceAllowance(currentUser);
        var remainingStandardWorkspaceSlots = standardWorkspaceAllowance == int.MaxValue ?
            0 :
            Math.Max(standardWorkspaceAllowance - standardWorkspaces.Count, 0);
        var hasOnlyExplorerOrNoWorkspaces = standardWorkspaces.Count == 0;

        var viewModel = new WorkspaceSelectorViewModel
        {
            Workspaces = workspaces,
            ExplorerWorkspace = explorerWorkspace,
            StandardWorkspaces = standardWorkspaces,
            UserIsConfirmed = currentUser.EmailConfirmed,
            CanCreateExplorerWorkspace = explorerWorkspace is null,
            StandardWorkspaceAllowance = standardWorkspaceAllowance,
            RemainingStandardWorkspaceSlots = remainingStandardWorkspaceSlots,
            CanCreateUnlimitedWorkspaces = currentUser.SubscriptionLevel == SubscriptionLevel.Agency,
            HasOnlyExplorerOrNoWorkspaces = hasOnlyExplorerOrNoWorkspaces,
            HasAnyStandardPurchase = currentUser.StandardWorkspaceAllowance > 0 || currentUser.SubscriptionLevel == SubscriptionLevel.Standard,
            StandardCheckoutUrl = _stripeConfig?.CheckoutLinks?.GetValueOrDefault(nameof(SubscriptionLevel.Standard)) ?? string.Empty,
            AgencyCheckoutUrl = _stripeConfig?.CheckoutLinks?.GetValueOrDefault(nameof(SubscriptionLevel.Agency)) ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpGet("create")]
    public async Task<IActionResult> CreateWorkspace(WorkspacePlan workspacePlan = WorkspacePlan.Standard)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var workspaceLimitReached = workspacePlan switch
        {
            WorkspacePlan.Explorer => await HasExplorerWorkspaceAsync(currentUser.Id),
            WorkspacePlan.Standard => !await CanCreateStandardWorkspaceAsync(currentUser),
            _ => true
        };
        
        ViewData["WorkspaceLimitReached"] = workspaceLimitReached;

        return View(new CreateWorkspace
        {
            WorkspacePlan = workspacePlan
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNewWorkspace(CreateWorkspace form)
    {
        if (!ModelState.IsValid)
        {
            return View("CreateWorkspace", form);
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

        var workspaceLimitReached = form.WorkspacePlan switch
        {
            WorkspacePlan.Explorer => await HasExplorerWorkspaceAsync(currentUser.Id),
            WorkspacePlan.Standard => !await CanCreateStandardWorkspaceAsync(currentUser),
            _ => true
        };
        
        if (workspaceLimitReached)
        {
            ViewData["WorkspaceLimitReached"] = true;
            var errorMessage = form.WorkspacePlan == WorkspacePlan.Explorer ?
                "You already have an Explorer workspace." :
                "No Standard workspace slots are available. Purchase a Standard or Agency workspace plan to continue.";
            ModelState.AddModelError(string.Empty, errorMessage);
            return View("CreateWorkspace", form);
        }

        var helper = new SlugHelper();
        var slug = helper.GenerateSlug(form.WorkspaceName);

        var existingSlug = await dbContext.Workspaces.FirstOrDefaultAsync(w => w.Slug == slug);
        if (existingSlug is not null)
        {
            // Handle duplicate (e.g., append a number or return an error)
            ModelState.AddModelError("WorkspaceName", "A workspace with a similar name already exists.");
            return View("CreateWorkspace", form);
        }

        var workspace = new Workspace
        {
            Name = form.WorkspaceName,
            Slug = slug,
            IsExplorer = form.WorkspacePlan == WorkspacePlan.Explorer,
            OwnerUserId = currentUser.Id
        };

        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(
            "Index", 
            "WorkspaceSelector"
        );
    }

    private async Task<bool> HasExplorerWorkspaceAsync(string userId)
    {
        return await dbContext.Workspaces
            .AnyAsync(workspace => workspace.IsExplorer && (workspace.OwnerUserId == userId || workspace.Users.Any(u => u.Id == userId)));
    }

    private async Task<bool> CanCreateStandardWorkspaceAsync(ApplicationUser currentUser)
    {
        var standardWorkspaceAllowance = GetStandardWorkspaceAllowance(currentUser);
        if (standardWorkspaceAllowance == int.MaxValue)
        {
            return true;
        }

        var standardWorkspaceCount = await dbContext.Workspaces
            .CountAsync(workspace => !workspace.IsExplorer && (workspace.OwnerUserId == currentUser.Id || workspace.Users.Any(u => u.Id == currentUser.Id)));

        return standardWorkspaceCount < standardWorkspaceAllowance;
    }

    private static int GetStandardWorkspaceAllowance(ApplicationUser user)
    {
        if (user.SubscriptionLevel == SubscriptionLevel.Agency)
        {
            return int.MaxValue;
        }

        if (user.StandardWorkspaceAllowance > 0)
        {
            return user.StandardWorkspaceAllowance;
        }

        return user.SubscriptionLevel == SubscriptionLevel.Standard ? 1 : 0;
    }
}
