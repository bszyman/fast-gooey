namespace FastGooey.Features.Interfaces.AppleTv.Product.Models;

public class AppleTvProductJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public string PreviewMediaUrl { get; set; } = string.Empty;
    
    public List<AppleTvProductRelatedItemJsonModel> RelatedProducts { get; set; } = [];
}

public class AppleTvProductRelatedItemJsonModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
}