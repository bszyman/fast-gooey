
using System.Text.Json.Serialization;

namespace FastGooey.Models.JsonDataModels.Mac;

public class MacContentJsonDataModel
{
    public string HeaderTitle { get; set; } = string.Empty;
    public string HeaderBackgroundImage { get; set; } = string.Empty;
    public List<MacContentItemJsonDataModel> Items { get; set; } = [];
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(HeadlineContentItem), "headline")]
[JsonDerivedType(typeof(LinkContentItem), "link")]
[JsonDerivedType(typeof(TextContentItem), "text")]
[JsonDerivedType(typeof(ImageContentItem), "image")]
[JsonDerivedType(typeof(VideoContentItem), "video")]
public abstract class MacContentItemJsonDataModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
}

public class HeadlineContentItem: MacContentItemJsonDataModel
{
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentItem: MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TextContentItem: MacContentItemJsonDataModel
{
    public string Text { get; set; } = string.Empty;
}

public class ImageContentItem: MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
}

public class VideoContentItem: MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}