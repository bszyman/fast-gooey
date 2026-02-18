namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaWorkspaceViewModel
{
    public Guid WorkspaceId { get; set; }
    public MediaSourceListItemViewModel Source { get; set; } = new();
    public string? CurrentPath { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<MediaBreadcrumbViewModel> Breadcrumbs { get; set; } = [];
    public IReadOnlyList<MediaItemViewModel> Items { get; set; } = [];
}
