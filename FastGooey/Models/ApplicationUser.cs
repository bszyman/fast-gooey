using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Models;

[Index(nameof(PublicId), IsUnique = true)]
[Index(nameof(WorkspaceId))]
public class ApplicationUser : IdentityUser
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

    [MaxLength(255)]
    public string? StripeCustomerId { get; set; }

    [MaxLength(255)]
    public string? StripeSubscriptionId { get; set; }

    public SubscriptionLevel SubscriptionLevel { get; set; } = SubscriptionLevel.Explorer;

    public ICollection<PasskeyCredential> PasskeyCredentials { get; set; } = new List<PasskeyCredential>();
    public ICollection<MagicLinkToken> MagicLinkTokens { get; set; } = new List<MagicLinkToken>();

    public bool PasskeyRequired { get; set; }

    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
