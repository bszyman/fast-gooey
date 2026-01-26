namespace FastGooey.Models.ViewModels.WorkspaceSelector;

public class WorkspaceSelectorViewModel
{
    public IEnumerable<Workspace> Workspaces { get; set; } = new List<Workspace>();
    public bool UserIsConfirmed { get; set; } = false;
}