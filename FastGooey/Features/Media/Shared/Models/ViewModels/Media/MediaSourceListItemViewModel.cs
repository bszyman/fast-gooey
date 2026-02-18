namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaSourceListItemViewModel
{
    public Guid SourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? DetailLine { get; set; }
    public string? StatusLine { get; set; }
    public IReadOnlyList<MediaFolderViewModel> RootFolders { get; set; } = [];
}
