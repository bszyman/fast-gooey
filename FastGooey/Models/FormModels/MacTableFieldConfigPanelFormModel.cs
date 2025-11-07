using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

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