using FastGooey.Models.JsonDataModels;

namespace FastGooey.HypermediaResponses;

public class AppleMobileCollectionHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleMobile";
    public string View { get; set; } = "Collection";

    public List<AppleMobileCollectionItemResponse> Content { get; set; } = [];
}

public class AppleMobileCollectionItemResponse
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public AppleMobileCollectionItemResponse()
    {
    }

    public AppleMobileCollectionItemResponse(AppleMobileCollectionViewItemJsonDataModel model)
    {
        Identifier = model.Identifier;
        Title = model.Title;
        ImageUrl = model.ImageUrl;
        Url = model.Url;
    }
}
