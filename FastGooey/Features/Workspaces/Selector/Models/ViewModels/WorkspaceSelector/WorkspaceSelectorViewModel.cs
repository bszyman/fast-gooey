using FastGooey.Models;

namespace FastGooey.Features.Workspaces.Selector.Models.ViewModels.WorkspaceSelector;

public class WorkspaceSelectorViewModel
{
    public IEnumerable<Workspace> Workspaces { get; set; } = new List<Workspace>();
    public Workspace? ExplorerWorkspace { get; set; }
    public IEnumerable<Workspace> StandardWorkspaces { get; set; } = new List<Workspace>();
    public bool UserIsConfirmed { get; set; } = false;
    public bool CanCreateExplorerWorkspace { get; set; }
    public int StandardWorkspaceAllowance { get; set; }
    public int RemainingStandardWorkspaceSlots { get; set; }
    public int OwnedStandardWorkspaceCount { get; set; }
    public bool CanCreateUnlimitedWorkspaces { get; set; }
    public bool HasOnlyExplorerOrNoWorkspaces { get; set; }
    public bool HasAnyStandardPurchase { get; set; }
    public string StandardCheckoutUrl { get; set; } = string.Empty;
    public string AgencyCheckoutUrl { get; set; } = string.Empty;
}
