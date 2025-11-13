using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.RssFeed;

public class RssWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public RssFeedJsonDataModel? Data { get; set; }
    
    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToString();
    }
}