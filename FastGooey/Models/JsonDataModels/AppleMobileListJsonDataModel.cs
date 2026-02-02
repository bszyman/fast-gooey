
using FastGooey.Models.Common;

namespace FastGooey.Models.JsonDataModels;

public class AppleMobileListJsonDataModel
{
    public IEnumerable<AppleMobileListItemJsonDataModel> Items { get; set; } = new List<AppleMobileListItemJsonDataModel>();
}

public class AppleMobileListItemJsonDataModel : IdentifiableBase
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}