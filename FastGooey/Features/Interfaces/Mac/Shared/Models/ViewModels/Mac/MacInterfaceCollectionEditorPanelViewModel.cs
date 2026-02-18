using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceCollectionEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public MacCollectionViewItemJsonDataModel? Item { get; set; }
}
