using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class MacTableStructureWorkspaceFormModel
{
    [Required]
    public List<string> Headers { get; set; } = new();
}