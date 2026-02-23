using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.Alert.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/Alert")]
public class AppleTvAlertController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleTvAlertWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleTvAlertWorkspaceViewModel, AlertContentJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleTvAlertIndexViewModel
        {
            Workspace = await WorkspaceViewModelForInterfaceId(interfaceGuid)
        };

        return View(viewModel);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        return PartialView("Workspace", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] AppleTvAlertWorkspaceFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#workspaceEditor");

            viewModel.Data.Title = formModel.Title;
            viewModel.Data.Description = formModel.Description ?? string.Empty;

            return PartialView("Workspace", viewModel);
        }

        viewModel.Data.Title = formModel.Title.Trim();
        viewModel.Data.Description = (formModel.Description ?? string.Empty).Trim();

        viewModel.ContentNode!.Config = JsonSerializer.SerializeToDocument(viewModel.Data);
        await dbContext.SaveChangesAsync();

        return PartialView("Workspace", viewModel);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        if (workspace is null)
        {
            return NotFound();
        }

        var data = new AlertContentJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Alert",
            Name = "New Alert Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleTvAlertIndexViewModel
        {
            Workspace = new AppleTvAlertWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}/upper-button-editor-panel")]
    public async Task<IActionResult> UpperButtonEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AlertContentJsonDataModel>() ?? new AlertContentJsonDataModel();

        return PartialView("Partials/UpperButtonEditorPanel", new AppleTvAlertUpperButtonEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            UpperButtonText = data.UpperButtonText,
            UpperButtonLink = data.UpperButtonLink
        });
    }

    [HttpPost("{interfaceId}/upper-button-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUpperButtonEditorPanel(string interfaceId, [FromForm] AppleTvAlertUpperButtonEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AlertContentJsonDataModel>() ?? new AlertContentJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/UpperButtonEditorPanel", new AppleTvAlertUpperButtonEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                UpperButtonText = formModel.UpperButtonText,
                UpperButtonLink = formModel.UpperButtonLink
            });
        }

        data.UpperButtonText = formModel.UpperButtonText.Trim();
        data.UpperButtonLink = formModel.UpperButtonLink.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/UpperButtonEditorPanel", new AppleTvAlertUpperButtonEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            UpperButtonText = data.UpperButtonText,
            UpperButtonLink = data.UpperButtonLink
        });
    }

    [HttpGet("{interfaceId}/lower-button-editor-panel")]
    public async Task<IActionResult> LowerButtonEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AlertContentJsonDataModel>() ?? new AlertContentJsonDataModel();

        return PartialView("Partials/LowerButtonEditorPanel", new AppleTvAlertLowerButtonEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            LowerButtonText = data.LowerButtonText,
            LowerButtonLink = data.LowerButtonLink
        });
    }

    [HttpPost("{interfaceId}/lower-button-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLowerButtonEditorPanel(string interfaceId, [FromForm] AppleTvAlertLowerButtonEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AlertContentJsonDataModel>() ?? new AlertContentJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/LowerButtonEditorPanel", new AppleTvAlertLowerButtonEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                LowerButtonText = formModel.LowerButtonText,
                LowerButtonLink = formModel.LowerButtonLink
            });
        }

        data.LowerButtonText = formModel.LowerButtonText.Trim();
        data.LowerButtonLink = formModel.LowerButtonLink.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/LowerButtonEditorPanel", new AppleTvAlertLowerButtonEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            LowerButtonText = data.LowerButtonText,
            LowerButtonLink = data.LowerButtonLink
        });
    }
}
