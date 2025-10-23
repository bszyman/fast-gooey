using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FastGooey.Controllers;

public abstract class BaseStudioController(IKeyValueService keyValueService): 
    Controller
{
    protected Guid WorkspaceId { get; private set; }
    
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
        
        await next();
    }
}