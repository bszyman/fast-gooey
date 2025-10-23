using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class WorkspaceManagementModel
{
    [Required(ErrorMessage = "Workspace name is required")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Workspace name must be between 1 and 80 characters")]
    [Display(Name = "Workspace Name")]
    public string WorkspaceName { get; set; } = string.Empty;
}