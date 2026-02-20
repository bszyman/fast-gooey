using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.List.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/List")]
public class AppleTvListController(
    ILogger<AppleTvListController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleTvListWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleTvListWorkspaceViewModel, ListJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleTvInterfaceListViewModel
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

    [HttpGet("workspace/{interfaceId}/list-contents")]
    public async Task<IActionResult> ListContentWorkspace(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        return PartialView("ListContents", viewModel);
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

        var data = new ListJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "List",
            Name = "New List Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleTvInterfaceListViewModel
        {
            Workspace = new AppleTvListWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}/banner-editor-panel")]
    public async Task<IActionResult> BannerEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        return PartialView("Partials/BannerEditorPanel", new AppleTvListBannerEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Background = data.Banner.Background,
            Button = data.Banner.Button,
            Description = data.Banner.Description,
            HeroImg = data.Banner.HeroImg,
            Img = data.Banner.Img,
            Row = data.Banner.Row,
            Stack = data.Banner.Stack,
            Title = data.Banner.Title
        });
    }

    [HttpPost("{interfaceId}/banner-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBannerEditorPanel(string interfaceId, [FromForm] AppleTvListBannerEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/BannerEditorPanel", new AppleTvListBannerEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                Background = formModel.Background,
                Button = formModel.Button,
                Description = formModel.Description,
                HeroImg = formModel.HeroImg,
                Img = formModel.Img,
                Row = formModel.Row,
                Stack = formModel.Stack,
                Title = formModel.Title
            });
        }

        data.Banner = new Banner
        {
            Background = formModel.Background,
            Button = formModel.Button,
            Description = formModel.Description,
            HeroImg = formModel.HeroImg,
            Img = formModel.Img,
            Row = formModel.Row,
            Stack = formModel.Stack,
            Title = formModel.Title
        };

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/BannerEditorPanel", new AppleTvListBannerEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Background = data.Banner.Background,
            Button = data.Banner.Button,
            Description = data.Banner.Description,
            HeroImg = data.Banner.HeroImg,
            Img = data.Banner.Img,
            Row = data.Banner.Row,
            Stack = data.Banner.Stack,
            Title = data.Banner.Title
        });
    }

    [HttpGet("{interfaceId}/list-editor-panel")]
    public async Task<IActionResult> ListEditorPanel(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        return PartialView("Partials/ListEditorPanel", new AppleTvListHeaderEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Title = data.Header.Title,
            Description = data.Header.Description
        });
    }

    [HttpPost("{interfaceId}/list-editor-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveListEditorPanel(string interfaceId, [FromForm] AppleTvListHeaderEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/ListEditorPanel", new AppleTvListHeaderEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                Title = formModel.Title,
                Description = formModel.Description
            });
        }

        data.Header = new Header
        {
            Title = formModel.Title,
            Description = formModel.Description
        };

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/ListEditorPanel", new AppleTvListHeaderEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Title = data.Header.Title,
            Description = data.Header.Description
        });
    }

    [HttpGet("{interfaceId}/item-editor-panel/{itemIndex?}")]
    public async Task<IActionResult> ListItemEditorPanel(string interfaceId, int? itemIndex)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        var selectedIndex = itemIndex.GetValueOrDefault(-1);
        var item = selectedIndex >= 0 && selectedIndex < data.ListItems.Count
            ? data.ListItems[selectedIndex]
            : new ListItem();

        return PartialView("Partials/ListItemEditorPanel", new AppleTvListItemEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            ItemIndex = selectedIndex,
            PosterImage = item.PosterImage,
            Title = item.Title,
            LinkToUrl = item.LinkToUrl
        });
    }

    [HttpPost("{interfaceId}/item-editor-panel/{itemIndex?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveListItemEditorPanel(
        string interfaceId,
        int? itemIndex,
        [FromForm] AppleTvListItemEditorPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        var selectedIndex = itemIndex.GetValueOrDefault(-1);
        var isExistingItem = selectedIndex >= 0 && selectedIndex < data.ListItems.Count;

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            return PartialView("Partials/ListItemEditorPanel", new AppleTvListItemEditorPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                ItemIndex = selectedIndex,
                LinkToUrl = formModel.LinkToUrl,
                Title = formModel.Title,
                PosterImage = formModel.PosterImage
            });
        }

        if (!isExistingItem)
        {
            data.ListItems.Add(new ListItem());
            selectedIndex = data.ListItems.Count - 1;
        }

        data.ListItems[selectedIndex] = new ListItem
        {
            LinkToUrl = formModel.LinkToUrl,
            Title = formModel.Title,
            PosterImage = formModel.PosterImage,
        };

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        // Response.Headers.Append("HX-Trigger", "toggleEditor");
        //
        // return PartialView("Partials/ListItemEditorPanel", new AppleTvListItemEditorPanelViewModel
        // {
        //     WorkspaceId = contentNode.Workspace.PublicId,
        //     InterfaceId = contentNode.DocId,
        //     ItemIndex = selectedIndex,
        //     PosterImage = formModel.PosterImage,
        //     Title = formModel.Title,
        //     LinkToUrl = formModel.LinkToUrl
        // });
        
        Response.Headers.Append("HX-Retarget", "#workspaceEditor");
        Response.Headers.Append("HX-Trigger", "toggleEditor");
        
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        return PartialView("ListContents", viewModel);
    }

    [HttpDelete("{interfaceId}/item/{itemIndex:int}")]
    public async Task<IActionResult> DeleteListItem(string interfaceId, int itemIndex)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ListJsonDataModel>() ?? new ListJsonDataModel();

        if (itemIndex < 0 || itemIndex >= data.ListItems.Count)
        {
            return NotFound();
        }

        data.ListItems.RemoveAt(itemIndex);

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleTvListWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("ListContents", viewModel);
    }
}
