namespace FastGooey.Features.Workspaces.Management.Models.ViewModels;

public class WorkspaceUserViewModel
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}
