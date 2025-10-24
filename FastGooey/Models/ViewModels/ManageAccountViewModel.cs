using FastGooey.Models.FormModels;

namespace FastGooey.Models.ViewModels;

public class ManageAccountViewModel
{
    public ApplicationUser? User { get; set; }
    public AccountManagementFormModel? FormModel { get; set; }
}