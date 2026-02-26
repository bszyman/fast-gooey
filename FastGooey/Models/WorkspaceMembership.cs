using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Models;

[Index(nameof(UserId))]
[Index(nameof(WorkspaceId))]
[Index(nameof(UserId), nameof(WorkspaceId), IsUnique = true)]
public class WorkspaceMembership
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public long WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}
