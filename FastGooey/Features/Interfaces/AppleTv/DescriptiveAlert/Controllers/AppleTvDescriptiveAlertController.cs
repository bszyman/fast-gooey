using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/DescriptiveAlert")]
public class AppleTvDescriptiveAlertController(
    ILogger<AppleTvDescriptiveAlertController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "Headline",
        "BodyCopy"
    ];

    private async Task<DescriptiveAlertWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<DescriptiveAlertWorkspaceViewModel, DescriptiveAlertContentJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleTvDescriptiveAlertIndexViewModel
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
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] DescriptiveAlertWorkspaceFormModel formModel)
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
            viewModel.Data.CancelButtonText = formModel.CancelButtonText;
            viewModel.Data.ConfirmButtonText = formModel.ConfirmButtonText;

            return PartialView("Workspace", viewModel);
        }

        viewModel.Data.Title = formModel.Title.Trim();
        viewModel.Data.CancelButtonText = formModel.CancelButtonText.Trim();
        viewModel.Data.ConfirmButtonText = formModel.ConfirmButtonText.Trim();

        viewModel.ContentNode!.Config = JsonSerializer.SerializeToDocument(viewModel.Data);
        await dbContext.SaveChangesAsync();

        return PartialView("Workspace", viewModel);
    }

    [HttpGet("{interfaceId}/descriptive-content")]
    public async Task<IActionResult> DescriptiveContentPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<DescriptiveAlertContentJsonDataModel>() ?? new DescriptiveAlertContentJsonDataModel();

        return PartialView("Partials/DescriptiveContentPanel", BuildDescriptiveContentViewModel(contentNode, data.DescriptiveContent));
    }

    [HttpPost("{interfaceId}/descriptive-content")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDescriptiveContentPanel(
        string interfaceId,
        [FromForm] DescriptiveAlertDescriptiveContentFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<DescriptiveAlertContentJsonDataModel>() ?? new DescriptiveAlertContentJsonDataModel();
        var normalizedNodes = NormalizeAndValidateDescriptiveContent(formModel);

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView(
                "Partials/DescriptiveContentPanel",
                new DescriptiveAlertDescriptiveContentViewModel
                {
                    WorkspaceId = contentNode.Workspace.PublicId,
                    InterfaceId = contentNode.DocId,
                    DescriptiveContent = normalizedNodes
                });
        }

        data.DescriptiveContent = normalizedNodes
            .Where(x => !string.IsNullOrWhiteSpace(x.Type) || !string.IsNullOrWhiteSpace(x.Content))
            .Select(x => new DescriptiveAlertContentNodeJsonDataModel
            {
                Type = x.Type,
                Content = x.Content
            })
            .ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/DescriptiveContentPanel", BuildDescriptiveContentViewModel(contentNode, data.DescriptiveContent));
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

        var data = new DescriptiveAlertContentJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "DescriptiveAlert",
            Name = "New Descriptive Alert Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleTvDescriptiveAlertIndexViewModel
        {
            Workspace = new DescriptiveAlertWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    private List<DescriptiveAlertDescriptiveContentNodeFormModel> NormalizeAndValidateDescriptiveContent(
        DescriptiveAlertDescriptiveContentFormModel formModel)
    {
        var normalizedNodes = formModel.DescriptiveContent
            .Select(x => new DescriptiveAlertDescriptiveContentNodeFormModel
            {
                Type = (x.Type ?? string.Empty).Trim(),
                Content = (x.Content ?? string.Empty).Trim()
            })
            .ToList();

        for (var i = 0; i < normalizedNodes.Count; i++)
        {
            var node = normalizedNodes[i];
            var hasType = !string.IsNullOrWhiteSpace(node.Type);
            var hasContent = !string.IsNullOrWhiteSpace(node.Content);

            if (!hasType && !hasContent)
            {
                continue;
            }

            if (!hasType)
            {
                ModelState.AddModelError($"DescriptiveContent[{i}].Type", "Type is required.");
                continue;
            }

            if (!AllowedContentTypes.Contains(node.Type))
            {
                ModelState.AddModelError($"DescriptiveContent[{i}].Type", "Type must be Headline or BodyCopy.");
            }

            if (!hasContent)
            {
                ModelState.AddModelError($"DescriptiveContent[{i}].Content", "Content is required.");
            }
        }

        return normalizedNodes;
    }

    private static DescriptiveAlertDescriptiveContentViewModel BuildDescriptiveContentViewModel(
        GooeyInterface contentNode,
        IEnumerable<DescriptiveAlertContentNodeJsonDataModel> nodes)
    {
        return new DescriptiveAlertDescriptiveContentViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            DescriptiveContent = nodes
                .Select(x => new DescriptiveAlertDescriptiveContentNodeFormModel
                {
                    Type = x.Type,
                    Content = x.Content
                })
                .ToList()
        };
    }
}
