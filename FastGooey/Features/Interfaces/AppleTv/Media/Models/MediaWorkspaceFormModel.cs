using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Media.Models;

public class MediaWorkspaceFormModel
{
    [Required]
    public string MediaUrl { get; set; } = string.Empty;
}
