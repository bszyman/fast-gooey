using FastGooey.Models.JsonDataModels;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceTableFieldEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string FieldAlias { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
}