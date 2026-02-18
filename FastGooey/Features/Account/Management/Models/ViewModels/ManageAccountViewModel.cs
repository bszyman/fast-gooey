using FastGooey.Features.Account.Management.Models.FormModels;
using FastGooey.Models;
using FastGooey.Models.ViewModels;

namespace FastGooey.Features.Account.Management.Models.ViewModels;

public class ManageAccountViewModel
{
    public ApplicationUser? User { get; set; }
    public AccountManagementFormModel? FormModel { get; set; }
    public Workspace? Workspace { get; set; }
    public string? DeleteAccountErrorMessage { get; set; }
    
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
    public IReadOnlyList<PasskeyCredential> Passkeys { get; set; } = Array.Empty<PasskeyCredential>();
}
