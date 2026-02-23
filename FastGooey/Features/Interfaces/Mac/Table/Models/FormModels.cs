using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Table.Models;

public class MacTableFieldConfigPanelFormModel
{
    [Required]
    public string FieldName { get; set; } = string.Empty;
    [Required]
    public string FieldAlias { get; set; } = string.Empty;
    [Required]
    public string FieldType { get; set; } = string.Empty;

    public List<string> DropdownOptions = [];
}

public class MacTableItemEditorPanelFormModel
{
    [Required]
    public string GooeyName { get; set; } = string.Empty;

    public string RelatedUrl { get; set; } = string.Empty;

    public string DoubleClickUrl { get; set; } = string.Empty;
}

public class MacTableStructureWorkspaceFormModel
{
    [Required]
    public List<string> Headers { get; set; } = new();
}
