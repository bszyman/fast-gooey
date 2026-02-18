using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceTableItemEditorPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }

    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = new();
    public MacTableItemJsonDataModel? Content { get; set; }
}