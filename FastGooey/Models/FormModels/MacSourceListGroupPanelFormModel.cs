using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class MacSourceListGroupPanelFormModel
{
    [Required]
    public string GroupName { get; set; } = string.Empty;
}