using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class AccountManagementController(
    IKeyValueService keyValueService,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid workspaceId)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();
        
        var workspace = await dbContext.Workspaces.FirstOrDefaultAsync(
            x => x.PublicId == workspaceId
        );

        var viewModel = CreateViewModel(currentUser);
        viewModel.NavBarViewModel = new MetalNavBarViewModel
        {
            WorkspaceName = workspace.Name,
            WorkspaceId = workspace.PublicId,
            ActiveTab = "My Account"
        };

        return View(viewModel);
    }

    [HttpGet("Workspace")]
    public async Task<IActionResult> AccountWorkspace()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

        var viewModel = CreateViewModel(currentUser);

        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
    }

    [HttpPost("Workspace/Save")]
    public async Task<IActionResult> SaveAccountWorkspace([Bind(Prefix = "FormModel")] AccountManagementFormModel formModel)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

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
        viewModel.FormModel.IsSaved = true;

        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
    }

    [HttpPost("Passkeys/{passkeyId:guid}/Delete")]
    public async Task<IActionResult> DeletePasskey(Guid passkeyId)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

        var passkey = await dbContext.PasskeyCredentials
            .FirstOrDefaultAsync(p => p.Id == passkeyId && p.UserId == currentUser.Id);

        if (passkey is null)
            return NotFound();

        dbContext.PasskeyCredentials.Remove(passkey);
        await dbContext.SaveChangesAsync();

        var viewModel = CreateViewModel(currentUser);
        return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
    }

    [HttpDelete("Workspace/DeleteAccount")]
    public async Task<IActionResult> DeleteAccount()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
            return Unauthorized();

        try
        {
            dbContext.KeyValueStores.RemoveRange(dbContext.KeyValueStores.Where(x => x.Key.Contains(currentUser.Id)));

            if (currentUser.WorkspaceId.HasValue)
            {
                var workspace = await dbContext.Workspaces.FirstOrDefaultAsync(x => x.Id == currentUser.WorkspaceId.Value);
                if (workspace is not null)
                {
                    dbContext.KeyValueStores.RemoveRange(dbContext.KeyValueStores.Where(x => x.Key.Contains(workspace.PublicId.ToString())));
                    dbContext.Workspaces.Remove(workspace);
                }
                else
                {
                    dbContext.PasskeyCredentials.RemoveRange(dbContext.PasskeyCredentials.Where(x => x.UserId == currentUser.Id));
                    dbContext.MagicLinkTokens.RemoveRange(dbContext.MagicLinkTokens.Where(x => x.UserId == currentUser.Id));
                    dbContext.Users.Remove(currentUser);
                }
            }
            else
            {
                dbContext.PasskeyCredentials.RemoveRange(dbContext.PasskeyCredentials.Where(x => x.UserId == currentUser.Id));
                dbContext.MagicLinkTokens.RemoveRange(dbContext.MagicLinkTokens.Where(x => x.UserId == currentUser.Id));
                dbContext.Users.Remove(currentUser);
            }
            await dbContext.SaveChangesAsync();

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            Response.Headers["HX-Redirect"] = "/Home/Index";
            return Ok();
        }
        catch (Exception ex)
        {
            var viewModel = CreateViewModel(currentUser);
            viewModel.DeleteAccountErrorMessage = $"Unable to delete account: {ex.Message}";
            return PartialView("~/Views/AccountManagement/Workspaces/AccountManagement.cshtml", viewModel);
        }
    }

    private ManageAccountViewModel CreateViewModel(ApplicationUser user)
    {
        return new ManageAccountViewModel
        {
            User = user,
            Passkeys = dbContext.PasskeyCredentials
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToList(),
            Workspace = dbContext.Workspaces.FirstOrDefault(x => x.PublicId == WorkspaceId),
            FormModel = new AccountManagementFormModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
            }
        };
    }
}
