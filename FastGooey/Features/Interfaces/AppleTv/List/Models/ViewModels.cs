using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.List.Models;

public class AppleTvInterfaceListViewModel
{
    public AppleTvListWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class AppleTvListBannerEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Background { get; set; } = string.Empty;
    public string Button { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HeroImg { get; set; } = string.Empty;
    public string Img { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public string Stack { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class AppleTvListHeaderEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AppleTvListItemEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public int ItemIndex { get; set; } = -1;
    public string PosterImage { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LinkToUrl { get; set; } = string.Empty;
}

public class AppleTvListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public ListJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}