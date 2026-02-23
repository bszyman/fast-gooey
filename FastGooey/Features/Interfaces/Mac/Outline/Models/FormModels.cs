using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Outline.Models;

public class MacOutlineEditorPanelFormModel
{
    public string ParentName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; } = Guid.Empty;
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
}