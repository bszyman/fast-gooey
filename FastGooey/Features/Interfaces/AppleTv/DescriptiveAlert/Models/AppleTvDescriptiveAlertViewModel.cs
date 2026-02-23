using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class AppleTvDescriptiveAlertIndexViewModel
{
    public DescriptiveAlertWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.InterfaceId();
    }
}

public class DescriptiveAlertDescriptiveContentViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public List<DescriptiveAlertDescriptiveContentNodeFormModel> DescriptiveContent { get; set; } = [];
}


public class DescriptiveAlertWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public DescriptiveAlertContentJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}