using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class AccountManagementFormModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 80 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 80 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
}