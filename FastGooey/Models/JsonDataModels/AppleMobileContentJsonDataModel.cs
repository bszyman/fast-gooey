
using System.Text.Json.Serialization;
using FastGooey.Models.Common;

namespace FastGooey.Models.JsonDataModels;

public class AppleMobileContentJsonDataModel : IContentDataModel<AppleMobileContentItemJsonDataModel>
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
public class AppleMobileContentItemJsonDataModel : ContentItemBase
{
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
    public string Caption { get; set; } = string.Empty;
}

public class VideoContentItem : AppleMobileContentItemJsonDataModel
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}