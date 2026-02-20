namespace FastGooey.Features.Media.LinkEditor.Models.ViewModels.LinkEditor;

public class LinkEditorViewModel
{
    public Guid? WorkspaceId { get; set; }
    public List<LinkEditorContentNode> AppleMobileNodes { get; set; } = [];
    public List<LinkEditorContentNode> MacNodes { get; set; } = [];
    public List<LinkEditorContentNode> AppleTvNodes { get; set; } = [];
}

public class LinkEditorContentNode
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string ViewType { get; set; } = string.Empty;
}
