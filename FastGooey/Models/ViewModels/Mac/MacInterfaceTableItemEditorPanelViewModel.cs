using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableItemEditorPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }

    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = new();
    public MacTableItemJsonDataModel? Content { get; set; }
}