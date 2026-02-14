using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.HypermediaResponses;

public class MacCollectionHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Mac";
    public string View { get; set; } = "Collection";

    public List<MacCollectionItemResponse> Content { get; set; } = [];
}

public class MacCollectionItemResponse
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public MacCollectionItemResponse()
    {
    }

    public MacCollectionItemResponse(MacCollectionViewItemJsonDataModel model)
    {
        Identifier = model.Identifier;
        Title = model.Title;
        ImageUrl = model.ImageUrl;
        Url = model.Url;
    }
}
