using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Models;

[Index(nameof(DocId), IsUnique = true)]
[Index(nameof(WorkspaceId))]
public class GooeyInterface
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public Guid DocId { get; set; } = Guid.NewGuid();
    
    [Required]
    [ForeignKey(nameof(Workspace))]
    public long WorkspaceId { get; set; }
    
    // Navigation property
    public Workspace Workspace { get; set; } = null!;

    [MaxLength(20)]
    public string Platform { get; set; } = string.Empty;
    
    [MaxLength(30)]
    public string? ViewType { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "jsonb")]
    public JsonDocument Config { get; set; } = JsonDocument.Parse("{}");
    
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}