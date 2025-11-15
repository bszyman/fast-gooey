using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FastGooey.Controllers;

public abstract class BaseStudioController(
    IKeyValueService keyValueService, 
    ApplicationDbContext dbContext): 
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
}