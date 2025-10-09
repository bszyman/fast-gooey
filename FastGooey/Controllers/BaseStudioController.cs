using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FastGooey.Controllers;

public abstract class BaseStudioController(IKeyValueService keyValueService): Controller
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // Set MapKit token in ViewBag for _Layout.cshtml
        ViewBag.MapKitToken = await keyValueService.GetValueForKey(Constants.MapKitJwt);
        
        await next();
    }
}