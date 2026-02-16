using System.ComponentModel.DataAnnotations;
using FastGooey.Models.Common;

namespace FastGooey.Models.FormModels.Mac;

public class HeadlineContentFormModel : ContentItemBase
{
    [Required]
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentFormModel : ContentItemBase
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class TextContentFormModel : ContentItemBase
{
    public string Text { get; set; } = string.Empty;
}

public class ImageContentFormModel : ContentItemBase
{
    public string Url { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}

public class VideoContentFormModel : ContentItemBase
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}
