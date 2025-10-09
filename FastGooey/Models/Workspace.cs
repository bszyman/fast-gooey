using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Models;

[Index(nameof(PublicId), IsUnique = true)]
public class Workspace
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public Guid PublicId { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;
    
    // Collection navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<GooeyInterface> GooeyInterfaces { get; set; } = new List<GooeyInterface>();
    
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}