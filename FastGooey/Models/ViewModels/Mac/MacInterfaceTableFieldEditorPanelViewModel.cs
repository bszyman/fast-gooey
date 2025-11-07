using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableFieldEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    
    public string FieldName { get; set; } = string.Empty;
    public string FieldAlias { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
}