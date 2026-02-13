using FastGooey.Models.Common;

namespace FastGooey.Models.JsonDataModels.Mac;

public class MacCollectionViewJsonDataModel
{
    public List<MacCollectionViewItemJsonDataModel> Items { get; set; } = [];
}

public class MacCollectionViewItemJsonDataModel : IdentifiableBase
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
