using FastGooey.Models.Common;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.Content.Models;

public class MacContentHeadlineConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public HeadlineContentItem? Content { get; set; }
}

public class MacContentImageConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public ImageContentItem? Content { get; set; }
}

public class MacContentLinkConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public LinkContentItem? Content { get; set; }
}

public class MacContentTextConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public TextContentItem? Content { get; set; }
}

public class MacContentTypeSelectorPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
}

public class MacContentVideoConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public VideoContentItem? Content { get; set; }
}

public class MacContentViewModel : ContentViewModelBase<MacContentWorkspaceViewModel>
{
    public override string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public override string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}

public class MacContentWorkspaceViewModel : ContentWorkspaceViewModelBase<MacContentJsonDataModel>
{
}