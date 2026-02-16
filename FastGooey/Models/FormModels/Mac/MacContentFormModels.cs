using System.ComponentModel.DataAnnotations;
using FastGooey.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels.Mac;

public class HeadlineContentFormModel : ContentItemBase
{
    [Required]
    public string Headline { get; set; } = string.Empty;
}

public class LinkContentFormModel : ContentItemBase
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "URL is required")]
    public string Url { get; set; } = string.Empty;
}

public class TextContentFormModel : ContentItemBase
{
    [Required]
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
