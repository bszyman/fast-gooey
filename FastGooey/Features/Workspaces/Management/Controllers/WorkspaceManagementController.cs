using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Media.Shared.Models.ViewModels.Media;
using FastGooey.Features.Workspaces.Management.Models.FormModels;
using FastGooey.Features.Workspaces.Management.Models.ViewModels;
using FastGooey.Models;
using FastGooey.Models.Configuration;
using FastGooey.Models.FormModels;
using FastGooey.Models.Media;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using FastGooey.Services.Media;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class WorkspaceManagementController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IMediaCredentialProtector mediaCredentialProtector,
    UserManager<ApplicationUser> userManager,
    EmailerService emailerService,
    IConfiguration configuration,
    IDataProtectionProvider dataProtectionProvider) :
    BaseStudioController(keyValueService, dbContext)
{
    private readonly IDataProtector _inviteProtector = dataProtectionProvider.CreateProtector("FastGooey.WorkspaceInvite");
    private readonly StripeConfigurationModel? _stripeConfig = configuration.GetSection("Stripe").Get<StripeConfigurationModel>();

    [HttpGet]
    public IActionResult Index()
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }

        var viewModel = CreateViewModel(workspace);
        viewModel.NavBarViewModel = new MetalNavBarViewModel
        {
            WorkspaceName = workspace.Name,
            WorkspaceId = workspace.PublicId,
            ActiveTab = "Workspace Settings"
        };

        return View("Index", viewModel);
    }

    [HttpGet("workspace")]
    public IActionResult OrganizationWorkspace()
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }
        
        return PartialView(
            "WorkspaceManagement", 
            CreateViewModel(workspace)
        );
    }

    [HttpPost("workspace/save")]
    public IActionResult EditWorkspace([Bind(Prefix = "FormModel")] WorkspaceManagementModel model)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }
        workspace.Name = model.WorkspaceName;

        dbContext.SaveChanges();

        var viewModel = CreateViewModel(workspace);
        viewModel.FormModel.IsSaved = true;

        return PartialView("WorkspaceManagement", viewModel);
    }

    [HttpGet("media-source-workspace")]
    public IActionResult MediaSourceEditor(Guid? sourceId)
    {
        //MediaSourceEditorViewModel
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }
        
        return PartialView(
            "MediaSourceConfiguration", 
            CreateViewModel(workspace)
        );
    }

    [HttpGet("media-source-workspace-panel")]
    public IActionResult MediaSourceEditorPanel(Guid? sourceId)
    {
        //MediaSourceEditorViewModel
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }
        
        var viewModel = BuildMediaSourceEditorViewModel(workspace, sourceId);

        return PartialView("Partials/MediaSourceEditorPanel", viewModel);
    }
    
    [HttpPost("media-source-workspace-panel/save")]
    public IActionResult SaveMediaSource([Bind(Prefix = "FormModel")] MediaSourceFormModel model)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var editorViewModel = BuildMediaSourceEditorViewModelFromMediaSourceModel(workspace, model);

            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/MediaSourceEditorPanel", editorViewModel);
        }

        var mediaSource = GetOrCreateMediaSource(workspace, model);
        ApplyMediaSourceForm(mediaSource, model);

        dbContext.SaveChanges();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces, toggleEditor");
        return PartialView(
            "MediaSourceConfiguration", 
            CreateViewModel(workspace)
        );
    }

    [HttpDelete("media-source-workspace/{sourceId:guid}")]
    public IActionResult DeleteMediaSource(Guid sourceId)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return Forbid();
        }

        var mediaSource = workspace.MediaSources.FirstOrDefault(source => source.PublicId == sourceId);
        if (mediaSource is null)
        {
            return NotFound();
        }

        dbContext.MediaSources.Remove(mediaSource);
        dbContext.SaveChanges();
        
        Response.Headers.Append("HX-Trigger", "refreshInterfaces, toggleEditor");
        return PartialView(
            "MediaSourceConfiguration", 
            CreateViewModel(workspace)
        );
    }

    [HttpGet("users")]
    public IActionResult ManageUsers()
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return WorkspaceUsersAccessDenied();
        }

        return PartialView("ManageUsers", CreateViewModel(workspace));
    }

    [HttpPost("users/invite")]
    public async Task<IActionResult> InviteWorkspaceUser([Bind(Prefix = "InviteUserFormModel")] WorkspaceUserInviteFormModel model)
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return WorkspaceUsersAccessDenied();
        }

        if (!CanManageWorkspaceUsers(workspace))
        {
            return PartialView("ManageUsers", CreateViewModel(workspace));
        }

        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Email = model.Email.Trim();

        if (ModelState.IsValid)
        {
            var existingUser = await userManager.FindByEmailAsync(model.Email);
            var existingMembership = existingUser is not null &&
                                     await dbContext.WorkspaceMemberships.AnyAsync(m =>
                                         m.WorkspaceId == workspace.Id &&
                                         m.UserId == existingUser.Id);
            if (existingMembership ||
                existingUser?.WorkspaceId == workspace.Id ||
                string.Equals(existingUser?.Id, workspace.OwnerUserId, StringComparison.Ordinal))
            {
                ModelState.AddModelError("InviteUserFormModel.Email", "That user is already in this workspace.");
            }
        }

        if (!ModelState.IsValid)
        {
            var invalidViewModel = CreateViewModel(workspace);
            invalidViewModel.InviteUserFormModel = model;
            return PartialView("ManageUsers", invalidViewModel);
        }

        var tokenPayload = JsonSerializer.Serialize(new WorkspaceInvitePayload
        {
            WorkspaceId = workspace.PublicId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email
        });
        var token = _inviteProtector.Protect(tokenPayload);
        var inviteLink = Url.Action(
            "Accept",
            "WorkspaceInvite",
            new { token },
            Request.Scheme);

        if (!string.IsNullOrWhiteSpace(inviteLink))
        {
            await emailerService.SendWorkspaceInviteEmail(
                model.Email,
                model.FirstName,
                model.LastName,
                workspace.Name,
                inviteLink);
        }

        var viewModel = CreateViewModel(workspace);
        viewModel.InviteUserFormModel = new WorkspaceUserInviteFormModel { IsSaved = true };
        return PartialView("ManageUsers", viewModel);
    }

    [HttpDelete("users/{userId:guid}")]
    public IActionResult RemoveWorkspaceUser(Guid userId)
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);
        if (!IsWorkspaceOwner(workspace))
        {
            return WorkspaceUsersAccessDenied();
        }

        if (!CanManageWorkspaceUsers(workspace))
        {
            return PartialView("ManageUsers", CreateViewModel(workspace));
        }

        var user = dbContext.Users.FirstOrDefault(x => x.PublicId == userId);
        if (user is null)
        {
            return NotFound();
        }

        var membershipRows = dbContext.WorkspaceMemberships
            .Where(membership => membership.WorkspaceId == workspace.Id && membership.UserId == user.Id)
            .ToList();

        var removedMembership = membershipRows.Count > 0;
        if (removedMembership)
        {
            dbContext.WorkspaceMemberships.RemoveRange(membershipRows);
        }

        var removedLegacyMembership = false;
        if (user.WorkspaceId == workspace.Id)
        {
            user.WorkspaceId = null;
            removedLegacyMembership = true;
        }

        if (!removedMembership && !removedLegacyMembership)
        {
            return NotFound();
        }

        dbContext.SaveChanges();

        return PartialView("ManageUsers", CreateViewModel(workspace));
    }

    private ManageWorkspaceViewModel CreateViewModel(Workspace workspace)
    {
        var workspaceUsers = dbContext.WorkspaceMemberships
            .Where(membership => membership.WorkspaceId == workspace.Id)
            .Select(membership => membership.User)
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Select(user => new WorkspaceUserViewModel
            {
                UserId = user.PublicId,
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty
            })
            .ToList();

        var legacyWorkspaceUsers = dbContext.Users
            .Where(user => user.WorkspaceId == workspace.Id)
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Select(user => new WorkspaceUserViewModel
            {
                UserId = user.PublicId,
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty
            })
            .ToList();

        foreach (var legacyUser in legacyWorkspaceUsers)
        {
            if (workspaceUsers.All(user => user.UserId != legacyUser.UserId))
            {
                workspaceUsers.Add(legacyUser);
            }
        }

        if (!string.IsNullOrWhiteSpace(workspace.OwnerUserId))
        {
            var owner = dbContext.Users.FirstOrDefault(user => user.Id == workspace.OwnerUserId);
            if (owner is not null)
            {
                var existingOwnerRow = workspaceUsers.FirstOrDefault(user => user.UserId == owner.PublicId);
                if (existingOwnerRow is not null)
                {
                    existingOwnerRow.IsOwner = true;
                }
                else
                {
                    workspaceUsers.Insert(0, new WorkspaceUserViewModel
                    {
                        UserId = owner.PublicId,
                        Name = $"{owner.FirstName} {owner.LastName}".Trim(),
                        Email = owner.Email ?? string.Empty,
                        IsOwner = true
                    });
                }
            }
        }

        return new ManageWorkspaceViewModel
        {
            Workspace = workspace,
            FormModel = new WorkspaceManagementModel
            {
                WorkspaceName = workspace.Name
            },
            MediaSources = workspace.MediaSources
                .OrderBy(source => source.Name)
                .Select(BuildMediaSourceSummary)
                .ToList(),
            MediaSourceEditor = BuildMediaSourceEditorViewModel(workspace, null),
            WorkspaceUsers = workspaceUsers,
            CanManageWorkspaceUsers = CanManageWorkspaceUsers(workspace),
            StandardCheckoutUrl = _stripeConfig?.CheckoutLinks?.GetValueOrDefault(nameof(SubscriptionLevel.Standard)) ?? string.Empty
        };
    }

    private bool IsWorkspaceOwner(Workspace workspace)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return false;
        }

        if (workspace.OwnerUserId == currentUserId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(workspace.OwnerUserId))
        {
            return false;
        }

        // Legacy fallback: before owner_user_id existed, membership used AspNetUsers.workspace_id.
        // If the current user is linked there, promote them to explicit owner to repair data in place.
        var isLegacyOwner = dbContext.Users.Any(user =>
            user.Id == currentUserId &&
            user.WorkspaceId == workspace.Id);

        if (!isLegacyOwner)
        {
            return false;
        }

        workspace.OwnerUserId = currentUserId;
        dbContext.SaveChanges();
        return true;
    }

    private bool CanManageWorkspaceUsers(Workspace workspace)
    {
        // Explorer workspaces can only ever have a single user
        if (!workspace.IsExplorer)
        {
            return true;
        }

        var currentUser = dbContext.Users.FirstOrDefault(user => user.Id == workspace.OwnerUserId);
        return currentUser?.SubscriptionLevel == SubscriptionLevel.Agency;
    }

    private IActionResult WorkspaceUsersAccessDenied()
    {
        Response.Headers.Append("HX-Retarget", "#workspace");
        return PartialView("Partials/WorkspaceUsersAccessDenied");
    }

    private sealed class WorkspaceInvitePayload
    {
        public Guid WorkspaceId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private MediaSourceSummaryViewModel BuildMediaSourceSummary(Models.Media.MediaSource source)
    {
        return new MediaSourceSummaryViewModel
        {
            SourceId = source.PublicId,
            Name = source.Name,
            SourceType = source.SourceType.ToDisplayName(),
            DetailLine = BuildSourceDetailLine(source),
            IsEnabled = source.IsEnabled,
            HasCredentials = HasStoredCredentials(source)
        };
    }

    private string? BuildSourceDetailLine(Models.Media.MediaSource source)
    {
        return source.SourceType switch
        {
            Models.Media.MediaSourceType.S3 => string.IsNullOrWhiteSpace(source.S3BucketName) ? null : $"S3 - {source.S3BucketName}",
            Models.Media.MediaSourceType.AzureBlob => string.IsNullOrWhiteSpace(source.AzureContainerName) ? null : $"Azure - {source.AzureContainerName}",
            Models.Media.MediaSourceType.WebDav => string.IsNullOrWhiteSpace(source.WebDavBaseUrl) ? null : source.WebDavBaseUrl,
            _ => null
        };
    }

    private bool HasStoredCredentials(Models.Media.MediaSource source)
    {
        return source.SourceType switch
        {
            Models.Media.MediaSourceType.S3 => !string.IsNullOrWhiteSpace(source.S3AccessKeyId) || !string.IsNullOrWhiteSpace(source.S3SecretAccessKey),
            Models.Media.MediaSourceType.AzureBlob => !string.IsNullOrWhiteSpace(source.AzureConnectionString),
            Models.Media.MediaSourceType.WebDav => !string.IsNullOrWhiteSpace(source.WebDavUsername) || !string.IsNullOrWhiteSpace(source.WebDavPassword),
            _ => false
        };
    }
    
    private MediaSourceEditorViewModel BuildMediaSourceEditorViewModel(Workspace workspace, Guid? sourceId)
    {
        if (sourceId is null)
        {
            return new MediaSourceEditorViewModel
            {
                WorkspaceId = workspace.PublicId,
                FormModel = new MediaSourceFormModel()
            };
        }

        var source = workspace.MediaSources.FirstOrDefault(item => item.PublicId == sourceId);
        if (source is null)
        {
            return new MediaSourceEditorViewModel
            {
                WorkspaceId = workspace.PublicId,
                FormModel = new MediaSourceFormModel()
            };
        }

        return new MediaSourceEditorViewModel
        {
            WorkspaceId = workspace.PublicId,
            IsEditing = true,
            HasStoredS3Credentials = !string.IsNullOrWhiteSpace(source.S3AccessKeyId) || !string.IsNullOrWhiteSpace(source.S3SecretAccessKey),
            HasStoredAzureConnectionString = !string.IsNullOrWhiteSpace(source.AzureConnectionString),
            HasStoredWebDavCredentials = !string.IsNullOrWhiteSpace(source.WebDavUsername) || !string.IsNullOrWhiteSpace(source.WebDavPassword),
            FormModel = new MediaSourceFormModel
            {
                MediaSourceId = source.PublicId,
                Name = source.Name,
                SourceType = source.SourceType,
                IsEnabled = source.IsEnabled,
                S3BucketName = source.S3BucketName,
                S3Region = source.S3Region,
                S3ServiceUrl = source.S3ServiceUrl,
                AzureContainerName = source.AzureContainerName,
                WebDavBaseUrl = source.WebDavBaseUrl,
                WebDavUseBasicAuth = source.WebDavUseBasicAuth
            }
        };
    }

    private MediaSourceEditorViewModel BuildMediaSourceEditorViewModelFromMediaSourceModel(Workspace workspace, MediaSourceFormModel model)
    {
        var viewModel = BuildMediaSourceEditorViewModel(workspace, model.MediaSourceId);
        viewModel.FormModel = model;
        viewModel.IsEditing = model.MediaSourceId.HasValue;
        return viewModel;
    }

    private Models.Media.MediaSource GetOrCreateMediaSource(Workspace workspace, MediaSourceFormModel model)
    {
        if (model.MediaSourceId.HasValue)
        {
            var existing = workspace.MediaSources.FirstOrDefault(source => source.PublicId == model.MediaSourceId.Value);
            if (existing is not null)
            {
                return existing;
            }
        }

        var created = new Models.Media.MediaSource
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };
        workspace.MediaSources.Add(created);
        dbContext.MediaSources.Add(created);
        return created;
    }

    private void ApplyMediaSourceForm(Models.Media.MediaSource source, MediaSourceFormModel model)
    {
        source.Name = model.Name.Trim();
        source.SourceType = model.SourceType;
        source.IsEnabled = model.IsEnabled;

        source.S3BucketName = model.SourceType == Models.Media.MediaSourceType.S3 ? model.S3BucketName?.Trim() : null;
        source.S3Region = model.SourceType == Models.Media.MediaSourceType.S3 ? model.S3Region?.Trim() : null;
        source.S3ServiceUrl = model.SourceType == Models.Media.MediaSourceType.S3 ? model.S3ServiceUrl?.Trim() : null;
        if (model.SourceType == Models.Media.MediaSourceType.S3 && !string.IsNullOrWhiteSpace(model.S3AccessKeyId))
        {
            source.S3AccessKeyId = mediaCredentialProtector.Protect(model.S3AccessKeyId);
        }
        if (model.SourceType == Models.Media.MediaSourceType.S3 && !string.IsNullOrWhiteSpace(model.S3SecretAccessKey))
        {
            source.S3SecretAccessKey = mediaCredentialProtector.Protect(model.S3SecretAccessKey);
        }
        if (model.SourceType != Models.Media.MediaSourceType.S3)
        {
            source.S3AccessKeyId = null;
            source.S3SecretAccessKey = null;
        }

        source.AzureContainerName = model.SourceType == Models.Media.MediaSourceType.AzureBlob ? model.AzureContainerName?.Trim() : null;
        if (model.SourceType == Models.Media.MediaSourceType.AzureBlob && !string.IsNullOrWhiteSpace(model.AzureConnectionString))
        {
            source.AzureConnectionString = mediaCredentialProtector.Protect(model.AzureConnectionString);
        }
        if (model.SourceType != Models.Media.MediaSourceType.AzureBlob)
        {
            source.AzureConnectionString = null;
        }

        source.WebDavBaseUrl = model.SourceType == Models.Media.MediaSourceType.WebDav ? model.WebDavBaseUrl?.Trim() : null;
        source.WebDavUseBasicAuth = model.SourceType == Models.Media.MediaSourceType.WebDav && model.WebDavUseBasicAuth;
        if (model.SourceType == Models.Media.MediaSourceType.WebDav && !string.IsNullOrWhiteSpace(model.WebDavUsername))
        {
            source.WebDavUsername = mediaCredentialProtector.Protect(model.WebDavUsername);
        }
        if (model.SourceType == Models.Media.MediaSourceType.WebDav && !string.IsNullOrWhiteSpace(model.WebDavPassword))
        {
            source.WebDavPassword = mediaCredentialProtector.Protect(model.WebDavPassword);
        }
        if (model.SourceType != Models.Media.MediaSourceType.WebDav)
        {
            source.WebDavUsername = null;
            source.WebDavPassword = null;
            source.WebDavUseBasicAuth = false;
        }
    }
}
