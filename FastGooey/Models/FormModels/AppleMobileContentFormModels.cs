namespace FastGooey.Models.FormModels;

public class HeadlineContentFormModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentFormModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TextContentFormModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Text { get; set; } = string.Empty;
}

public class ImageContentFormModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
}

public class VideoContentFormModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}