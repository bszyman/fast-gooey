using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastGooey.Utils;
using FastGooey.Extensions;

namespace FastGooey.Controllers.Interfaces;

public abstract class BaseInterfaceController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) : BaseStudioController(keyValueService, dbContext)
{
    protected async Task<GooeyInterface> GetInterfaceAsync(Guid interfaceId)
    {
        return await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
    }

    protected async Task<TViewModel> GetInterfaceViewModelAsync<TViewModel, TDataModel>(Guid interfaceId)
        where TViewModel : new()
    {
        var contentNode = await GetInterfaceAsync(interfaceId);
        var viewModel = new TViewModel();

        // Use reflection or a common property if possible, but for now, we'll keep it simple
        // since ViewModels don't share a common base yet.
        var nodeProp = typeof(TViewModel).GetProperty("ContentNode") ?? typeof(TViewModel).GetProperty("Workspace");
        nodeProp?.SetValue(viewModel, contentNode);

        var dataProp = typeof(TViewModel).GetProperty("Data");
        if (dataProp is not null)
        {
            dataProp.SetValue(viewModel, contentNode.Config.DeserializePolymorphic<TDataModel>());
        }

        return viewModel;
    }

    protected bool TryParseInterfaceId(string interfaceId, out Guid interfaceGuid)
    {
        return GuidShortId.TryParse(interfaceId, out interfaceGuid);
    }
}
