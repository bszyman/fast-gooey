namespace FastGooey.Models.ViewModels.Media;

public class MediaSourceListViewModel
{
    public Guid WorkspaceId { get; set; }
    public IReadOnlyList<MediaSourceListItemViewModel> Sources { get; set; } = [];
}
