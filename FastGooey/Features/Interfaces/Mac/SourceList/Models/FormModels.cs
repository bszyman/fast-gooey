using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.SourceList.Models;

public class MacSourceListGroupItemPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Icon { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;
}

public class MacSourceListGroupPanelFormModel
{
    [Required]
    public string GroupName { get; set; } = string.Empty;
}
