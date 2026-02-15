using FastGooey.Models.Common;

namespace FastGooey.Models.JsonDataModels.Mac;

public class MacSourceListJsonDataModel
{
    public List<MacSourceListGroupJsonDataModel> Groups { get; set; } = [];
}

public class MacSourceListGroupJsonDataModel : IdentifiableBase
{
    public string GroupName { get; set; } = string.Empty;
    public List<MacSourceListGroupItemJsonDataModel> GroupItems { get; set; } = [];
}

public class MacSourceListGroupItemJsonDataModel : IdentifiableBase
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
