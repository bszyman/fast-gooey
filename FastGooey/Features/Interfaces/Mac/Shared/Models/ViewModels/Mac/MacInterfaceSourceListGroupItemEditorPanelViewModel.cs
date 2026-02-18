using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceSourceListGroupItemEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    public Guid GroupId { get; set; } = Guid.Empty;

    public Guid Identifier { get; set; } = Guid.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;
}
