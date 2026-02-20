namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;

public class AppleTvListItemEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public int ItemIndex { get; set; } = -1;
    public string PosterImage { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LinkToUrl { get; set; } = string.Empty;
}
