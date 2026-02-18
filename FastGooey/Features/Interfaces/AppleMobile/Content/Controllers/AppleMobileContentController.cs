using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Extensions;
using FastGooey.Features.Interfaces.AppleMobile.Shared.Models.FormModels;
using FastGooey.Features.Interfaces.AppleMobile.Shared.Models.ViewModels.AppleMobileInterface;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleMobile.Content.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/Content")]
public class AppleMobileContentController(
    ILogger<AppleMobileContentController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    ContentInterfaceControllerBase<AppleMobileContentJsonDataModel, AppleMobileContentItemJsonDataModel, AppleMobileContentWorkspaceViewModel, AppleMobileContentWorkspaceFormModel>(keyValueService, dbContext)
{
    protected override string Platform => "AppleMobile";
    protected override string ViewType => "Content";
    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        var viewModel = new AppleMobileContentViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        return View(viewModel);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new AppleMobileContentJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = Platform,
            ViewType = ViewType,
            Name = $"New {ViewType} Interface",
            Config = JsonSerializer.SerializeToDocument(data, JsonDocumentExtensions.PolymorphicOptions)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleMobileContentViewModel
        {
            WorkspaceViewModel = new AppleMobileContentWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}/content-type-selector-panel")]
    public IActionResult ContentTypeSelectorPanel(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleMobileContentTypeSelectorPanelViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceId = interfaceGuid
        };

        return PartialView("Partials/AppleMobileContentTypeSelectorPanel", viewModel);
    }

    [HttpGet("{interfaceId}/headline-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> HeadlineConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<HeadlineContentItem, AppleMobileContentHeadlineConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "Partials/ContentHeadlineConfigurationPanel",
            () => new AppleMobileContentHeadlineConfigurationPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid
            },
            (vm, content) => vm.Content = content
        );
    }

    [HttpPost("{interfaceId}/headline-item/{itemId:guid?}")]
    public async Task<IActionResult> SaveHeadline(Guid workspaceId, string interfaceId, Guid? itemId, HeadlineContentFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return await LoadConfigurationPanelInternal<HeadlineContentItem, AppleMobileContentHeadlineConfigurationPanelViewModel>(
                interfaceGuid,
                itemId,
                "Partials/ContentHeadlineConfigurationPanel",
                () => new AppleMobileContentHeadlineConfigurationPanelViewModel
                {
                    WorkspaceId = WorkspaceId,
                    InterfaceId = interfaceGuid
                },
                (vm, content) => vm.Content = content
            );
        }

        return await SaveContentItemInternal<HeadlineContentItem, HeadlineContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "headline",
            (item, f) => item.Headline = f.Headline
        );
    }

    [HttpGet("{interfaceId}/link-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> LinkConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<LinkContentItem, AppleMobileLinkConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "Partials/ContentLinkConfigurationPanel",
            () => new AppleMobileLinkConfigurationPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid
            },
            (vm, content) => vm.Content = content
        );
    }

    [HttpPost("{interfaceId}/link-item/{itemId:guid?}")]
    public async Task<IActionResult> SaveLink(Guid workspaceId, string interfaceId, Guid? itemId, LinkContentFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }
        
        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return await LoadConfigurationPanelInternal<LinkContentItem, AppleMobileLinkConfigurationPanelViewModel>(
                interfaceGuid,
                itemId,
                "Partials/ContentLinkConfigurationPanel",
                () => new AppleMobileLinkConfigurationPanelViewModel
                {
                    WorkspaceId = WorkspaceId,
                    InterfaceId = interfaceGuid
                },
                (vm, content) => vm.Content = content
            );
        }

        return await SaveContentItemInternal<LinkContentItem, LinkContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "link",
            (item, f) =>
            {
                item.Title = f.Title;
                item.Url = f.Url;
            }
        );
    }

    [HttpGet("{interfaceId}/text-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> TextConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<TextContentItem, AppleMobileTextConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "Partials/ContentTextConfigurationPanel",
            () => new AppleMobileTextConfigurationPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid,
            },
            (vm, content) => vm.Content = content
        );
    }

    [HttpPost("{interfaceId}/text-item/{itemId:guid?}")]
    public async Task<IActionResult> SaveText(Guid workspaceId, string interfaceId, Guid? itemId, TextContentFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return await LoadConfigurationPanelInternal<TextContentItem, AppleMobileTextConfigurationPanelViewModel>(
                interfaceGuid,
                itemId,
                "Partials/ContentTextConfigurationPanel",
                () => new AppleMobileTextConfigurationPanelViewModel
                {
                    WorkspaceId = WorkspaceId,
                    InterfaceId = interfaceGuid,
                },
                (vm, content) => vm.Content = content
            );
        }

        return await SaveContentItemInternal<TextContentItem, TextContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "text",
            (item, f) => item.Text = f.Text
        );
    }

    [HttpGet("{interfaceId}/image-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> ImageConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<ImageContentItem, AppleMobileImageConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "Partials/ContentImageConfigurationPanel",
            () => new AppleMobileImageConfigurationPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid,
            },
            (vm, content) => vm.Content = content
        );
    }

    [HttpPost("{interfaceId}/image-item/{itemId:guid?}")]
    public async Task<IActionResult> SaveImage(Guid workspaceId, string interfaceId, Guid? itemId, ImageContentFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }
        
        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return await LoadConfigurationPanelInternal<ImageContentItem, AppleMobileImageConfigurationPanelViewModel>(
                interfaceGuid,
                itemId,
                "Partials/ContentImageConfigurationPanel",
                () => new AppleMobileImageConfigurationPanelViewModel
                {
                    WorkspaceId = WorkspaceId,
                    InterfaceId = interfaceGuid,
                },
                (vm, content) => vm.Content = content
            );
        }

        return await SaveContentItemInternal<ImageContentItem, ImageContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "image",
            (item, f) =>
            {
                item.Url = f.Url;
                item.Caption = f.Caption;
            }
        );
    }

    [HttpGet("{interfaceId}/video-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> VideoConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<VideoContentItem, AppleMobileVideoConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "Partials/ContentVideoConfigurationPanel",
            () => new AppleMobileVideoConfigurationPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid,
            },
            (vm, content) => vm.Content = content
        );
    }

    [HttpPost("{interfaceId}/video-item/{itemId:guid?}")]
    public async Task<IActionResult> SaveVideo(Guid workspaceId, string interfaceId, Guid? itemId, VideoContentFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await SaveContentItemInternal<VideoContentItem, VideoContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "video",
            (item, f) =>
            {
                item.Url = f.Url;
                item.ThumbnailUrl = f.ThumbnailUrl;
            }
        );
    }

    [HttpPost("{interfaceId}/reorder-items")]
    public async Task<IActionResult> ReorderItems(Guid workspaceId, string interfaceId, ContentItemOrderFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.DeserializePolymorphic<AppleMobileContentJsonDataModel>();

        if (form.OrderedItemIds.Count > 0)
        {
            var itemsById = data.Items.ToDictionary(item => item.Identifier);
            var reordered = new List<AppleMobileContentItemJsonDataModel>();

            foreach (var itemId in form.OrderedItemIds)
            {
                if (itemsById.TryGetValue(itemId, out var item))
                {
                    reordered.Add(item);
                    itemsById.Remove(itemId);
                }
            }

            foreach (var item in data.Items)
            {
                if (itemsById.ContainsKey(item.Identifier))
                {
                    reordered.Add(item);
                    itemsById.Remove(item.Identifier);
                }
            }

            data.Items = reordered;
            contentNode.Config = JsonSerializer.SerializeToDocument(data, JsonDocumentExtensions.PolymorphicOptions);
            await dbContext.SaveChangesAsync();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        return PartialView("Workspace", viewModel);
    }

    [HttpDelete("{interfaceId}/item/{itemId:guid}")]
    public Task<IActionResult> DeleteItem(Guid workspaceId, string interfaceId, Guid itemId)
    {
        return DeleteItemInternal(interfaceId, itemId);
    }
}
