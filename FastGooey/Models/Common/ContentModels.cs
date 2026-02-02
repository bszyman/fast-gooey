using System.Text.Json.Serialization;
using FastGooey.Utils;

namespace FastGooey.Models.Common;

public abstract class IdentifiableBase
{
    public Guid Identifier { get; set; } = Guid.Empty;
}

public abstract class ContentItemBase : IdentifiableBase
{
    public string ContentType { get; set; } = string.Empty;
}

public interface IContentDataModel<TItem> where TItem : ContentItemBase
{
    string HeaderTitle { get; set; }
    string HeaderBackgroundImage { get; set; }
    List<TItem> Items { get; set; }
}

public abstract class ContentWorkspaceViewModelBase<TDataModel>
{
    public GooeyInterface? ContentNode { get; set; }
    public TDataModel Data { get; set; } = default!;

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}

public abstract class ContentViewModelBase<TWorkspaceViewModel>
{
    public TWorkspaceViewModel? WorkspaceViewModel { get; set; }

    public abstract string WorkspaceId();
    public abstract string InterfaceId();
}

public abstract class ContentWorkspaceFormModelBase
{
    public string HeaderTitle { get; set; } = string.Empty;
    public string HeaderBackgroundImage { get; set; } = string.Empty;
}
