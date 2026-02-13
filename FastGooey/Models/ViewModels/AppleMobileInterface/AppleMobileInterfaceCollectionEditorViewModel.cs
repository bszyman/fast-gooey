using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceCollectionEditorViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public AppleMobileCollectionViewItemJsonDataModel? Item { get; set; }
}
