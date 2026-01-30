using System.Security.Claims;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

public abstract class BaseStudioController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    Controller
{
    protected Guid WorkspaceId { get; private set; }
    protected Guid InterfaceId { get; private set; }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        ViewBag.MapKitToken = await keyValueService.GetValueForKey(Constants.MapKitJwt);

        if (context.RouteData.Values.TryGetValue("workspaceId", out var idValue))
        {
            if (idValue is string idString && Guid.TryParse(idString, out var id))
            {
                WorkspaceId = id;
                ViewData["WorkspaceId"] = id;
            }
        }

        if (context.RouteData.Values.TryGetValue("interfaceId", out var interfaceIdValue))
        {
            if (interfaceIdValue is string idString && Guid.TryParse(idString, out var id))
            {
                InterfaceId = id;
                ViewData["InterfaceId"] = id;
            }
        }

        await next();
    }

    protected Workspace? GetWorkspace()
    {
        return dbContext.Workspaces.FirstOrDefault(x => x.PublicId.Equals(WorkspaceId));
    }

    protected async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
    }

    protected async Task<bool> InterfaceLimitReachedAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || currentUser.SubscriptionLevel != SubscriptionLevel.Explorer)
        {
            return false;
        }

        var workspace = GetWorkspace();
        if (workspace is null)
        {
            return false;
        }

        return await dbContext.GooeyInterfaces.AnyAsync(node => node.WorkspaceId == workspace.Id);
    }
}
