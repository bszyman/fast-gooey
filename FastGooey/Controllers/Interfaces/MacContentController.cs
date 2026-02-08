using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Extensions;
using FastGooey.Models;
using FastGooey.Models.FormModels.Mac;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Models.ViewModels.Mac;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeadlineContentFormModel = FastGooey.Models.FormModels.Mac.HeadlineContentFormModel;
using HeadlineContentItem = FastGooey.Models.JsonDataModels.Mac.HeadlineContentItem;
using ImageContentFormModel = FastGooey.Models.FormModels.Mac.ImageContentFormModel;
using ImageContentItem = FastGooey.Models.JsonDataModels.Mac.ImageContentItem;
using LinkContentFormModel = FastGooey.Models.FormModels.Mac.LinkContentFormModel;
using LinkContentItem = FastGooey.Models.JsonDataModels.Mac.LinkContentItem;
using TextContentFormModel = FastGooey.Models.FormModels.Mac.TextContentFormModel;
using TextContentItem = FastGooey.Models.JsonDataModels.Mac.TextContentItem;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/interfaces/mac/content")]
public class MacContentController(
    ILogger<MacContentController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    ContentInterfaceControllerBase<MacContentJsonDataModel, MacContentItemJsonDataModel, MacContentWorkspaceViewModel, MacContentWorkspaceFormModel>(keyValueService, dbContext)
{
    protected override string Platform => "Mac";
    protected override string ViewType => "Content";
    protected override string BaseViewPath => "~/Views/MacContent";

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        var viewModel = new MacContentViewModel
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
        var data = new MacContentJsonDataModel();

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

        var viewModel = new MacContentViewModel
        {
            WorkspaceViewModel = new MacContentWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshNavigation");

        return PartialView($"{BaseViewPath}/Index.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/content-type-selector-panel")]
    public IActionResult ContentTypeSelectorPanel(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new MacContentTypeSelectorPanelViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceId = interfaceGuid
        };

        return PartialView($"{BaseViewPath}/Partials/ContentTypeSelectorPanel.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/headline-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> HeadlineConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanelInternal<HeadlineContentItem, MacContentHeadlineConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            $"{BaseViewPath}/Partials/ContentHeadlineConfigurationPanel.cshtml",
            () => new MacContentHeadlineConfigurationPanelViewModel
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

        return await LoadConfigurationPanelInternal<LinkContentItem, MacContentLinkConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            $"{BaseViewPath}/Partials/ContentLinkConfigurationPanel.cshtml",
            () => new MacContentLinkConfigurationPanelViewModel
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

        return await LoadConfigurationPanelInternal<TextContentItem, MacContentTextConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            $"{BaseViewPath}/Partials/ContentTextConfigurationPanel.cshtml",
            () => new MacContentTextConfigurationPanelViewModel
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

        return await LoadConfigurationPanelInternal<ImageContentItem, MacContentImageConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            $"{BaseViewPath}/Partials/ContentImageConfigurationPanel.cshtml",
            () => new MacContentImageConfigurationPanelViewModel
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

        return await SaveContentItemInternal<ImageContentItem, ImageContentFormModel>(
            interfaceGuid,
            itemId,
            form,
            "image",
            (item, f) =>
            {
                item.Url = f.Url;
                item.AltText = f.AltText;
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

        return await LoadConfigurationPanelInternal<VideoContentItem, MacContentVideoConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            $"{BaseViewPath}/Partials/ContentVideoConfigurationPanel.cshtml",
            () => new MacContentVideoConfigurationPanelViewModel
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

    [HttpDelete("{interfaceId}/item/{itemId:guid}")]
    public Task<IActionResult> DeleteItem(Guid workspaceId, string interfaceId, Guid itemId)
    {
        return DeleteItemInternal(interfaceId, itemId);
    }
}
