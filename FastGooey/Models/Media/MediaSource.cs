using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Models.Media;

[Index(nameof(PublicId), IsUnique = true)]
public class MediaSource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public Guid PublicId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public MediaSourceType SourceType { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    public long WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    [MaxLength(200)]
    public string? S3BucketName { get; set; }

    [MaxLength(64)]
    public string? S3Region { get; set; }

    [MaxLength(2000)]
    public string? S3ServiceUrl { get; set; }

    [MaxLength(4000)]
    public string? S3AccessKeyId { get; set; }

    [MaxLength(4000)]
    public string? S3SecretAccessKey { get; set; }

    [MaxLength(4000)]
    public string? AzureConnectionString { get; set; }

    [MaxLength(200)]
    public string? AzureContainerName { get; set; }

    [MaxLength(2000)]
    public string? WebDavBaseUrl { get; set; }

    [MaxLength(4000)]
    public string? WebDavUsername { get; set; }

    [MaxLength(4000)]
    public string? WebDavPassword { get; set; }

    public bool WebDavUseBasicAuth { get; set; }

    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
