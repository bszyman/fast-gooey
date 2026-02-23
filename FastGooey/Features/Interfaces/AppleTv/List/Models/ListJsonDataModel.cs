using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;

public class ListJsonDataModel
{
    public Banner Banner { get; set; } = new ();
    public Header Header { get; set; } = new ();
    public List<ListItem> ListItems { get; set; } = new ();
}

public class ListItem 
{
    public string Title { get; set; } = string.Empty;
    public string PosterImage { get; set; } = string.Empty;
    public string LinkToUrl { get; set; } = string.Empty;
}