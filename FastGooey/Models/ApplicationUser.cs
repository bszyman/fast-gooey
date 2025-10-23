using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Models;

[Index(nameof(PublicId), IsUnique = true)]
[Index(nameof(WorkspaceId))]
public class ApplicationUser: IdentityUser
{
    [Required]
    public Guid PublicId { get; set; } = Guid.NewGuid();
    
    [ForeignKey(nameof(Workspace))]
    public long? WorkspaceId { get; set; }
    
    // Navigation property
    public Workspace? Workspace { get; set; } = null!;
    
    [MaxLength(40)]
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(40)]
    public string LastName { get; set; } = string.Empty;
    
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}