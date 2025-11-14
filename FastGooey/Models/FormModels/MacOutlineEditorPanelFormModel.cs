namespace FastGooey.Models.FormModels;

public class MacOutlineEditorPanelFormModel
{
    public string ParentName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}