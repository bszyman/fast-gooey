
using System.Text.Json.Serialization;
using FastGooey.Models.Common;

namespace FastGooey.Features.Interfaces.Mac.Content.Models;

public class MacContentJsonDataModel : IContentDataModel<MacContentItemJsonDataModel>
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
public class MacContentItemJsonDataModel : ContentItemBase
{
}

public class HeadlineContentItem : MacContentItemJsonDataModel
{
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentItem : MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TextContentItem : MacContentItemJsonDataModel
{
    public string Text { get; set; } = string.Empty;
}

public class ImageContentItem : MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}

public class VideoContentItem : MacContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}