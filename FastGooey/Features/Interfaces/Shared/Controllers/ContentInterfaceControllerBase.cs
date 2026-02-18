using System.Text.Json;
using FastGooey.Database;
using FastGooey.Extensions;
using FastGooey.Models;
using FastGooey.Models.Common;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.Shared.Controllers;

public abstract class ContentInterfaceControllerBase<TDataModel, TItemBase, TWorkspaceViewModel, TWorkspaceFormModel>(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
    where TDataModel : class, IContentDataModel<TItemBase>, new()
    where TItemBase : ContentItemBase, new()
    where TWorkspaceViewModel : ContentWorkspaceViewModelBase<TDataModel>, new()
    where TWorkspaceFormModel : ContentWorkspaceFormModelBase
{
    protected abstract string Platform { get; }
    protected abstract string ViewType { get; }
    protected async Task<TWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<TWorkspaceViewModel, TDataModel>(interfaceId);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(Guid workspaceId, string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] TWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var data = viewModel.Data;

        data.HeaderTitle = formModel.HeaderTitle;
        data.HeaderBackgroundImage = formModel.HeaderBackgroundImage;

        viewModel.ContentNode!.Config = JsonSerializer.SerializeToDocument(data);

        await dbContext.SaveChangesAsync();

        return PartialView("Workspace", viewModel);
    }

    protected async Task<IActionResult> SaveContentItemInternal<TItem, TForm>(
        Guid interfaceId,
        Guid? itemId,
        TForm form,
        string contentType,
        Action<TItem, TForm> updateItem)
        where TItem : TItemBase, new()
    {
        var contentNode = await dbContext.GooeyInterfaces
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.DeserializePolymorphic<TDataModel>();

        if (ModelState.IsValid)
        {
            TItem? item = null;

            if (itemId.HasValue)
            {
                item = data.Items
                    .OfType<TItem>()
                    .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
            }

            if (item is null)
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
        return PartialView("Workspace", viewModel);
    }

    protected async Task<IActionResult> LoadConfigurationPanelInternal<TItem, TViewModel>(
        Guid interfaceId,
        Guid? itemId,
        string viewPath,
        Func<TViewModel> createViewModel,
        Action<TViewModel, TItem> setContent)
        where TItem : TItemBase, new()
        where TViewModel : class
    {
        var contentItem = new TItem();

        if (itemId.HasValue)
        {
            var contentNode = await dbContext.GooeyInterfaces
                .FirstAsync(x => x.DocId.Equals(interfaceId));

            var data = contentNode.Config.DeserializePolymorphic<TDataModel>();
            contentItem = data.Items
                .OfType<TItem>()
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value)) ?? new TItem();
        }

        var viewModel = createViewModel();
        setContent(viewModel, contentItem);

        return PartialView(viewPath, viewModel);
    }

    [NonAction]
    protected async Task<IActionResult> DeleteItemInternal(string interfaceId, Guid itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.DeserializePolymorphic<TDataModel>();
        var item = data.Items
            .FirstOrDefault(x => x.Identifier.Equals(itemId));

        if (item is null)
        {
            return NotFound();
        }

        data.Items.Remove(item);

        contentNode.Config = JsonSerializer.SerializeToDocument(data, JsonDocumentExtensions.PolymorphicOptions);
        await dbContext.SaveChangesAsync();

        var viewModel = new TWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Workspace", viewModel);
    }
}
