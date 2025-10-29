using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceListEditorViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public AppleMobileListItemJsonDataModel? Item { get; set; }
}