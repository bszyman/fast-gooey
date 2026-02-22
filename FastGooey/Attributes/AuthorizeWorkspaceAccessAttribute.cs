using System.Security.Claims;
using FastGooey.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeWorkspaceAccessAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Get the workspaceId from route
        if (!context.RouteData.Values.TryGetValue("workspaceId", out var workspaceIdValue) ||
            workspaceIdValue is not string workspaceIdString ||
            !Guid.TryParse(workspaceIdString, out var workspaceId))
        {
            context.Result = new BadRequestObjectResult("Invalid or missing workspaceId.");
            return;
        }

        // Get the current user's ID
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get the DbContext from DI
        var dbContext = context.HttpContext.RequestServices
            .GetRequiredService<ApplicationDbContext>();

        // Check if the user has access to this workspace.
        // The owner check is the current model; the legacy relation fallback keeps older data working.
        var hasAccess = await dbContext.Workspaces.AnyAsync(w =>
            w.PublicId == workspaceId &&
            (w.OwnerUserId == userId || w.Users.Any(u => u.Id == userId)));

        if (!hasAccess)
        {
            context.Result = new ForbidResult();
            return;
        }

        // User has access, continue execution
        await next();
    }
}
