using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace FastGooey.Models;

public class MagicLinkToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();

    public Instant CreatedAt { get; set; }

    public Instant ExpiresAt { get; set; }

    public Instant? UsedAt { get; set; }
}
