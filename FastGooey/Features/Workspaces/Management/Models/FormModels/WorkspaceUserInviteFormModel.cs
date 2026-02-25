using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Workspaces.Management.Models.FormModels;

public class WorkspaceUserInviteFormModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(40, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 40 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(40, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 40 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public bool IsSaved { get; set; }
}
