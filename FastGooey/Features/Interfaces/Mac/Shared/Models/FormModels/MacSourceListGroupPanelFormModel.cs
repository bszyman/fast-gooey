using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels;

public class MacSourceListGroupPanelFormModel
{
    [Required]
    public string GroupName { get; set; } = string.Empty;
}