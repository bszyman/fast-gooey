using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;

public class MediaWorkspaceFormModel
{
    public string Title { get; set; } = string.Empty;
}

public class MediaGridItemPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? LinkTo { get; set; }

    [Required]
    public string PreviewMedia { get; set; } = string.Empty;
}
