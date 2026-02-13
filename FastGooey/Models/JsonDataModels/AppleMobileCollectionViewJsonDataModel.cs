using FastGooey.Models.Common;

namespace FastGooey.Models.JsonDataModels;

public class AppleMobileCollectionViewJsonDataModel
{
    public IEnumerable<AppleMobileCollectionViewItemJsonDataModel> Items { get; set; } = new List<AppleMobileCollectionViewItemJsonDataModel>();
}

public class AppleMobileCollectionViewItemJsonDataModel : IdentifiableBase
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
