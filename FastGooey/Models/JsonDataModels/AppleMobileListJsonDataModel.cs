
namespace FastGooey.Models.JsonDataModels;

public class AppleMobileListJsonDataModel
{
    public IEnumerable<AppleMobileListItemJsonDataModel> Items { get; set; } = new List<AppleMobileListItemJsonDataModel>();
}

public class AppleMobileListItemJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;
}