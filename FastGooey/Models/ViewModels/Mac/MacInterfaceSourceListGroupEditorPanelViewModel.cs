using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceSourceListGroupEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    public Guid GroupId { get; set; } = Guid.Empty;
    
    [Required]
    public string GroupName { get; set; } = string.Empty;
}