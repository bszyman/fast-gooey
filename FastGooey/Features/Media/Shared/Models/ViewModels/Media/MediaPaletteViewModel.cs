namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaPaletteViewModel
{
    public Guid WorkspaceId { get; set; }
    public IReadOnlyList<MediaPaletteSourceViewModel> Sources { get; set; } = [];
}

public class MediaPaletteSourceViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid SourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? DetailLine { get; set; }
    public string? StatusLine { get; set; }
    public IReadOnlyList<MediaItemViewModel> Items { get; set; } = [];
    public bool HasMore { get; set; }
    public int RequestedCount { get; set; }
    public int NextCount { get; set; }
}
