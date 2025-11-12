using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class TableItemOptionRowViewModel
{
    public int? OptionRowCounter { get; set; }
    public string SelectedAlias { get; set; } = string.Empty;
    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = [];
}