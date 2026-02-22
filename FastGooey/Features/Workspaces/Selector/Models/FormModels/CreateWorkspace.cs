using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Workspaces.Selector.Models.FormModels;

public class CreateWorkspace
{
    [Required(ErrorMessage = "Workspace name is required")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Workspace name must be between 1 and 80 characters")]
    [Display(Name = "Workspace Name")]
    public string WorkspaceName { get; set; } = string.Empty;

    public WorkspacePlan WorkspacePlan { get; set; } = WorkspacePlan.Standard;
}
