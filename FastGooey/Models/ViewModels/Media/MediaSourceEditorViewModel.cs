using FastGooey.Models.FormModels;

namespace FastGooey.Models.ViewModels.Media;

public class MediaSourceEditorViewModel
{
    public Guid? WorkspaceId { get; set; }
    public MediaSourceFormModel FormModel { get; set; } = new();
    public bool HasStoredS3Credentials { get; set; }
    public bool HasStoredAzureConnectionString { get; set; }
    public bool HasStoredWebDavCredentials { get; set; }
    public bool IsEditing { get; set; }
}
