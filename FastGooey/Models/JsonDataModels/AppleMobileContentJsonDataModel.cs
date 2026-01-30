
using System.Text.Json.Serialization;

namespace FastGooey.Models.JsonDataModels;

public class AppleMobileContentJsonDataModel
{
    public string HeaderTitle { get; set; } = string.Empty;
    public string HeaderBackgroundImage { get; set; } = string.Empty;
    public List<AppleMobileContentItemJsonDataModel> Items { get; set; } = [];
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(HeadlineContentItem), "headline")]
[JsonDerivedType(typeof(LinkContentItem), "link")]
[JsonDerivedType(typeof(TextContentItem), "text")]
[JsonDerivedType(typeof(ImageContentItem), "image")]
[JsonDerivedType(typeof(VideoContentItem), "video")]
public abstract class AppleMobileContentItemJsonDataModel
{
    public string ContentType { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
}

public class HeadlineContentItem : AppleMobileContentItemJsonDataModel
{
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentItem : AppleMobileContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TextContentItem : AppleMobileContentItemJsonDataModel
{
    public string Text { get; set; } = string.Empty;
}

public class ImageContentItem : AppleMobileContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
}

public class VideoContentItem : AppleMobileContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}