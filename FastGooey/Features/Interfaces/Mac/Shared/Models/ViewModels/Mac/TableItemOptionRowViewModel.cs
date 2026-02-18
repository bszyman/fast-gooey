
using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class TableItemOptionRowViewModel
{
    public int? OptionRowCounter { get; set; }
    public string SelectedAlias { get; set; } = string.Empty;
    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = [];
}