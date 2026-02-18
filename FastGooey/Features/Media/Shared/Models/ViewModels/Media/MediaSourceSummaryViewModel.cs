namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaSourceSummaryViewModel
{
    public Guid SourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? DetailLine { get; set; }
    public bool IsEnabled { get; set; }
    public bool HasCredentials { get; set; }
}
