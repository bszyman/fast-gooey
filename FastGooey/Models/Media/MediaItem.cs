namespace FastGooey.Models.Media;

public class MediaItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
}
