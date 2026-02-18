namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaPreviewViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid SourceId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContentType { get; set; }
}
