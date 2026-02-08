namespace FastGooey.Models.ViewModels.Media;

public class MediaItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public bool IsImage { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
}
