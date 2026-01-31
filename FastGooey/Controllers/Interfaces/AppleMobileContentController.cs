using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Extensions;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels.AppleMobileInterface;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/Content")]
public class AppleMobileContentController(
    ILogger<AppleMobileContentController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    private async Task<AppleMobileContentWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var viewModel = new AppleMobileContentWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.DeserializePolymorphic<AppleMobileContentJsonDataModel>()
        };

        return viewModel;
    }

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

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/AppleMobileContent/Workspace.cshtml", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] AppleMobileContentWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var data = viewModel.Data;

        data.HeaderTitle = formModel.HeaderTitle;
        data.HeaderBackgroundImage = formModel.HeaderBackgroundImage;

        viewModel.ContentNode.Config = JsonSerializer.SerializeToDocument(data);

        await dbContext.SaveChangesAsync();

        return PartialView("~/Views/AppleMobileContent/Workspace.cshtml", viewModel);
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
            Platform = "AppleMobile",
            ViewType = "Content",
            Name = "New Content Interface",
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

        Response.Headers.Append("HX-Trigger", "refreshNavigation");

        return PartialView("~/Views/AppleMobileContent/Index.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/content-type-selector-panel")]
    public IActionResult ContentTypeSelectorPanel(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        // TODO: probably set up a AppleMobileContentJsonDataModel just to initialize before attempting to add child
        // content items, probably should do this in CreateInterface()

        var viewModel = new AppleMobileContentTypeSelectorPanelViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceId = interfaceGuid
        };

        return PartialView("~/Views/AppleMobileContent/Partials/AppleMobileContentTypeSelectorPanel.cshtml", viewModel);
    }

    private async Task<IActionResult> SaveContentItem<TItem, TForm>(
        Guid interfaceId,
        Guid? itemId,
        TForm form,
        string contentType,
        Action<TItem, TForm> updateItem)
        where TItem : AppleMobileContentItemJsonDataModel, new()
    {
        var contentNode = await dbContext.GooeyInterfaces
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.DeserializePolymorphic<AppleMobileContentJsonDataModel>();

        if (ModelState.IsValid)
        {
            TItem? item = null;

            if (itemId.HasValue)
            {
                item = data.Items
                    .OfType<TItem>()
                    .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
            }

            if (item == null)
            {
                item = new TItem
                {
                    ContentType = contentType,
                    Identifier = Guid.NewGuid()
                };
                data.Items = data.Items.Append(item).ToList();
            }

            updateItem(item, form);

            contentNode.Config = JsonSerializer.SerializeToDocument(data, JsonDocumentExtensions.PolymorphicOptions);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        return PartialView("~/Views/AppleMobileContent/Workspace.cshtml", viewModel);
    }

    private async Task<IActionResult> LoadConfigurationPanel<TItem, TViewModel>(
        Guid interfaceId,
        Guid? itemId,
        string viewPath,
        Func<TViewModel> createViewModel,
        Action<TViewModel, TItem> setContent)
        where TItem : AppleMobileContentItemJsonDataModel, new()
        where TViewModel : class
    {
        var contentItem = new TItem();

        if (itemId.HasValue)
        {
            var contentNode = await dbContext.GooeyInterfaces
                .FirstAsync(x => x.DocId.Equals(interfaceId));

            var data = contentNode.Config.DeserializePolymorphic<AppleMobileContentJsonDataModel>();
            contentItem = data.Items
                .OfType<TItem>()
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        }

        var viewModel = createViewModel();
        setContent(viewModel, contentItem);

        return PartialView(viewPath, viewModel);
    }

    [HttpGet("{interfaceId}/headline-config-panel/{itemId:guid?}")]
    public async Task<IActionResult> HeadlineConfigurationPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return await LoadConfigurationPanel<HeadlineContentItem, AppleMobileContentHeadlineConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "~/Views/AppleMobileContent/Partials/ContentHeadlineConfigurationPanel.cshtml",
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

        return await SaveContentItem<HeadlineContentItem, HeadlineContentFormModel>(
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

        return await LoadConfigurationPanel<LinkContentItem, AppleMobileLinkConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "~/Views/AppleMobileContent/Partials/ContentLinkConfigurationPanel.cshtml",
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

        return await SaveContentItem<LinkContentItem, LinkContentFormModel>(
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

        return await LoadConfigurationPanel<TextContentItem, AppleMobileTextConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "~/Views/AppleMobileContent/Partials/ContentTextConfigurationPanel.cshtml",
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

        return await SaveContentItem<TextContentItem, TextContentFormModel>(
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

        return await LoadConfigurationPanel<ImageContentItem, AppleMobileImageConfigurationPanelViewModel>(
            interfaceGuid,
            itemId,
            "~/Views/AppleMobileContent/Partials/ContentImageConfigurationPanel.cshtml",
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

        return await SaveContentItem<ImageContentItem, ImageContentFormModel>(
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

    [HttpDelete("{interfaceId}/item/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid workspaceId, string interfaceId, Guid itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.DeserializePolymorphic<AppleMobileContentJsonDataModel>();
        var item = data.Items
            .FirstOrDefault(x => x.Identifier.Equals(itemId));

        if (item == null)
        {
            return NotFound();
        }

        data.Items.Remove(item);

        contentNode.Config = JsonSerializer.SerializeToDocument(data, JsonDocumentExtensions.PolymorphicOptions);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleMobileContentWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("~/Views/AppleMobileContent/Workspace.cshtml", viewModel);
    }

    // [HttpGet("{interfaceId}/video-config-panel/{itemId:guid?}")]
    // public async Task<IActionResult> VideoConfigurationPanel(Guid interfaceId, Guid? itemId)
    // {
    //     return await LoadConfigurationPanel<VideoContentItem, AppleMobileVideoConfigurationPanelViewModel>(
    //         interfaceId,
    //         itemId,
    //         "~/Views/AppleMobileContent/Partials/ContentVideoConfigurationPanel.cshtml",
    //         () => new AppleMobileVideoConfigurationPanelViewModel
    //         {
    //             WorkspaceId = WorkspaceId,
    //             InterfaceId = interfaceId,
    //         },
    //         (vm, content) => vm.Content = content
    //     );
    // }
    //
    // [HttpPost("{interfaceId}/video-item/{itemId:guid?}")]
    // public async Task<IActionResult> SaveVideo(Guid workspaceId, Guid interfaceId, Guid? itemId, VideoContentFormModel form)
    // {
    //     return await SaveContentItem<VideoContentItem, VideoContentFormModel>(
    //         interfaceId,
    //         itemId,
    //         form,
    //         "video",
    //         (item, f) =>
    //         {
    //             item.Url = f.Url;
    //             item.ThumbnailUrl = f.ThumbnailUrl;
    //         }
    //     );
    // }

    // unfurl url
    // inline list
    // any widget
}
