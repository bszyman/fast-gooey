using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacContentJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToString();
    }
}