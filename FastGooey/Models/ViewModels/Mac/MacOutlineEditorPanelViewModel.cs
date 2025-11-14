namespace FastGooey.Models.ViewModels.Mac;

public class MacOutlineEditorPanelViewModel
{
    public string ParentName { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public Guid Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}