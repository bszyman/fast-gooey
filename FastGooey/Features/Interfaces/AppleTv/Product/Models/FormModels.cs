using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Product.Models;

public class ProductWorkspaceFormModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PreviewMediaUrl { get; set; } = string.Empty;
}

public class RelatedItemPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Link { get; set; } = string.Empty;

    [Required]
    public string MediaUrl { get; set; } = string.Empty;
}
