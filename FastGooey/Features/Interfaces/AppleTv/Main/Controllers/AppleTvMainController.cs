using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;
using FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.Main.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/Main")]
public class AppleTvMainController(
    ILogger<AppleTvMainController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleTvMainWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleTvMainWorkspaceViewModel, MainJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceIdAsGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleTvInterfaceMainViewModel
        {
            Workspace = await WorkspaceViewModelForInterfaceId(interfaceIdAsGuid)
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

        var data = new MainJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Main",
            Name = "New Main Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index");
    }

    [HttpGet("{interfaceId}/background-editor-panel")]
    public async Task<IActionResult> BackgroundEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MainJsonDataModel>() ?? new MainJsonDataModel();
        var viewModel = new AppleTvMainBackgroundEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            ImageResource = data.BackgroundSplash.ImageResource,
            AudioResource = data.BackgroundSplash.AudioResource
        };

        return PartialView("Partials/backgroundEditorPanel", viewModel);
    }

    [HttpPost("{interfaceId}/background-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBackgroundEditorPanel(string interfaceId, [FromForm] AppleTvMainBackgroundEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<MainJsonDataModel>() ?? new MainJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/backgroundEditorPanel", new AppleTvMainBackgroundEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                ImageResource = formModel.ImageResource,
                AudioResource = formModel.AudioResource
            });
        }

        data.BackgroundSplash = new BackgroundSplash
        {
            ImageResource = formModel.ImageResource,
            AudioResource = formModel.AudioResource
        };

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/backgroundEditorPanel", new AppleTvMainBackgroundEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            ImageResource = data.BackgroundSplash.ImageResource,
            AudioResource = data.BackgroundSplash.AudioResource
        });
    }

    [HttpGet("{interfaceId}/menubar-editor-panel")]
    public async Task<IActionResult> MenuBarEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<MainJsonDataModel>() ?? new MainJsonDataModel();

        return PartialView("Partials/menubarEditorPanel", new AppleTvMainMenuBarEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Items = data.MenuBarButtons.Select(x => new AppleTvMainMenuBarButtonViewModel
            {
                Text = x.Text,
                Link = x.Link
            }).ToList()
        });
    }

    [HttpPost("{interfaceId}/menubar-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMenuBarEditorPanel(string interfaceId, [FromForm] AppleTvMainMenuBarEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<MainJsonDataModel>() ?? new MainJsonDataModel();

        var items = formModel.Items.ToList();
        var action = formModel.Action;
        var actionIndex = formModel.Index;

        if (!string.IsNullOrWhiteSpace(action) && action.Contains(':', StringComparison.Ordinal))
        {
            var actionParts = action.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (actionParts.Length == 2 && int.TryParse(actionParts[1], out var parsedIndex))
            {
                action = actionParts[0];
                actionIndex = parsedIndex;
            }
        }

        if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
        {
            items.Add(new AppleTvMainMenuBarButtonFormModel());
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, items));
        }

        if (actionIndex >= 0 && actionIndex < items.Count)
        {
            if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                items.RemoveAt(actionIndex);
                Response.Headers.Append("HX-Retarget", "#editorPanel");
                return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, items));
            }

            if (action.Equals("up", StringComparison.OrdinalIgnoreCase) && actionIndex > 0)
            {
                (items[actionIndex - 1], items[actionIndex]) = (items[actionIndex], items[actionIndex - 1]);
                Response.Headers.Append("HX-Retarget", "#editorPanel");
                return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, items));
            }

            if (action.Equals("down", StringComparison.OrdinalIgnoreCase) && actionIndex < items.Count - 1)
            {
                (items[actionIndex + 1], items[actionIndex]) = (items[actionIndex], items[actionIndex + 1]);
                Response.Headers.Append("HX-Retarget", "#editorPanel");
                return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, items));
            }
        }

        var menuBarButtons = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Text) || !string.IsNullOrWhiteSpace(x.Link))
            .ToList();

        for (var i = 0; i < menuBarButtons.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(menuBarButtons[i].Text))
            {
                ModelState.AddModelError($"Items[{i}].Text", "Text is required.");
            }

            if (string.IsNullOrWhiteSpace(menuBarButtons[i].Link))
            {
                ModelState.AddModelError($"Items[{i}].Link", "Link is required.");
            }
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, items));
        }

        data.MenuBarButtons = menuBarButtons
            .Select(x => new NavigationButtonJsonDataModel
            {
                Text = x.Text.Trim(),
                Link = x.Link.Trim()
            }).ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        return PartialView("Partials/menubarEditorPanel", ToMenuBarViewModel(contentNode, menuBarButtons));
    }

    private static AppleTvMainMenuBarEditorPanelViewModel ToMenuBarViewModel(GooeyInterface contentNode, List<AppleTvMainMenuBarButtonFormModel> items)
    {
        return new AppleTvMainMenuBarEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Items = items.Select(x => new AppleTvMainMenuBarButtonViewModel
            {
                Text = x.Text,
                Link = x.Link
            }).ToList()
        };
    }
}
