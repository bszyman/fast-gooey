using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels;

public class MacTableStructureWorkspaceFormModel
{
    [Required]
    public List<string> Headers { get; set; } = new();
}