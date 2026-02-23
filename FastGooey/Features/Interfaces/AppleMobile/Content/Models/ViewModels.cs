using FastGooey.Models.Common;
using FastGooey.Models.JsonDataModels;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleMobile.Content.Models;

public class AppleMobileContentHeadlineConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public HeadlineContentItem? Content { get; set; }
}

public class AppleMobileImageConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public ImageContentItem? Content { get; set; }
}

public class AppleMobileLinkConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public LinkContentItem? Content { get; set; }
}

public class AppleMobileTextConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public TextContentItem? Content { get; set; }
}

public class AppleMobileContentTypeSelectorPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
}

public class AppleMobileVideoConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public VideoContentItem? Content { get; set; }
}

public class AppleMobileContentViewModel : ContentViewModelBase<AppleMobileContentWorkspaceViewModel>
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

public class AppleMobileContentWorkspaceViewModel : ContentWorkspaceViewModelBase<AppleMobileContentJsonDataModel>
{
}
