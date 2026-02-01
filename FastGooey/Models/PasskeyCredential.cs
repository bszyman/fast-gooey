using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace FastGooey.Models;

public class PasskeyCredential
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public byte[] DescriptorId { get; set; } = Array.Empty<byte>();

    [Required]
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();

    [Required]
    [MaxLength(32)]
    public string CredentialType { get; set; } = string.Empty;

    public uint SignatureCounter { get; set; }

    public Guid Aaguid { get; set; }

    public byte[]? UserHandle { get; set; }

    [MaxLength(120)]
    public string? DisplayName { get; set; }

    public Instant CreatedAt { get; set; }

    public Instant? LastUsedAt { get; set; }
}
