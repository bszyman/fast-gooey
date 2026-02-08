using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.Media;
using FastGooey.Models.ViewModels;
using FastGooey.Models.ViewModels.Media;
using FastGooey.Services;
using FastGooey.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class WorkspaceManagementController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IMediaCredentialProtector mediaCredentialProtector) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public IActionResult Index()
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);

        var viewModel = CreateViewModel(workspace);
        viewModel.NavBarViewModel = new MetalNavBarViewModel
        {
            WorkspaceName = workspace.Name,
            WorkspaceId = workspace.PublicId,
            ActiveTab = "Workspace Settings"
        };

        return View(viewModel);
    }

    [HttpGet("workspace")]
    public IActionResult OrganizationWorkspace()
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        
        return PartialView(
            "~/Views/WorkspaceManagement/WorkspaceManagement.cshtml", 
            CreateViewModel(workspace)
        );
    }

    [HttpPost("workspace/save")]
    public IActionResult EditWorkspace([Bind(Prefix = "FormModel")] WorkspaceManagementModel model)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        workspace.Name = model.WorkspaceName;

        dbContext.SaveChanges();

        var viewModel = CreateViewModel(workspace);
        viewModel.FormModel.IsSaved = true;

        return PartialView("~/Views/WorkspaceManagement/WorkspaceManagement.cshtml", viewModel);
    }

    [HttpGet("media-source-workspace")]
    public IActionResult MediaSourceEditor(Guid? sourceId)
    {
        //MediaSourceEditorViewModel
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);
        
        return PartialView(
            "~/Views/WorkspaceManagement/MediaSourceConfiguration.cshtml", 
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
        
        var viewModel = BuildMediaSourceEditorViewModel(workspace, sourceId);

        return PartialView("~/Views/WorkspaceManagement/Partials/MediaSourceEditorPanel.cshtml", viewModel);
    }
    
    [HttpPost("media-source-workspace-panel/save")]
    public IActionResult SaveMediaSource([Bind(Prefix = "FormModel")] MediaSourceFormModel model)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);

        if (!ModelState.IsValid)
        {
            var editorViewModel = BuildMediaSourceEditorViewModelFromMediaSourceModel(workspace, model);

            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("~/Views/WorkspaceManagement/Partials/MediaSourceEditorPanel.cshtml", editorViewModel);
        }

        var mediaSource = GetOrCreateMediaSource(workspace, model);
        ApplyMediaSourceForm(mediaSource, model);

        dbContext.SaveChanges();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces, toggleEditor");
        return PartialView(
            "~/Views/WorkspaceManagement/MediaSourceConfiguration.cshtml", 
            CreateViewModel(workspace)
        );
    }

    [HttpDelete("media-source-workspace/{sourceId:guid}")]
    public IActionResult DeleteMediaSource(Guid sourceId)
    {
        var workspace = dbContext.Workspaces
            .Include(w => w.MediaSources)
            .First(x => x.PublicId == WorkspaceId);

        var mediaSource = workspace.MediaSources.FirstOrDefault(source => source.PublicId == sourceId);
        if (mediaSource == null)
        {
            return NotFound();
        }

        dbContext.MediaSources.Remove(mediaSource);
        dbContext.SaveChanges();
        
        Response.Headers.Append("HX-Trigger", "refreshInterfaces, toggleEditor");
        return PartialView(
            "~/Views/WorkspaceManagement/MediaSourceConfiguration.cshtml", 
            CreateViewModel(workspace)
        );
    }

    private ManageWorkspaceViewModel CreateViewModel(Workspace workspace)
    {
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
            MediaSourceEditor = BuildMediaSourceEditorViewModel(workspace, null)
        };
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
        if (sourceId == null)
        {
            return new MediaSourceEditorViewModel
            {
                WorkspaceId = workspace.PublicId,
                FormModel = new MediaSourceFormModel()
            };
        }

        var source = workspace.MediaSources.FirstOrDefault(item => item.PublicId == sourceId);
        if (source == null)
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
            if (existing != null)
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
