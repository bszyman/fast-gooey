using System.ComponentModel.DataAnnotations;
using FastGooey.Models.Media;

namespace FastGooey.Models.FormModels;

public class MediaSourceFormModel
{
    public Guid? MediaSourceId { get; set; }

    [Required(ErrorMessage = "Source name is required")]
    [StringLength(120, MinimumLength = 1, ErrorMessage = "Source name must be between 1 and 120 characters")]
    [Display(Name = "Source Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Source Type")]
    public MediaSourceType SourceType { get; set; } = MediaSourceType.S3;

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;

    [Display(Name = "Bucket Name")]
    public string? S3BucketName { get; set; }

    [Display(Name = "Region")]
    public string? S3Region { get; set; }

    [Display(Name = "Service URL (Optional)")]
    public string? S3ServiceUrl { get; set; }

    [Display(Name = "Access Key ID")]
    public string? S3AccessKeyId { get; set; }

    [Display(Name = "Secret Access Key")]
    public string? S3SecretAccessKey { get; set; }

    [Display(Name = "Connection String")]
    public string? AzureConnectionString { get; set; }

    [Display(Name = "Container Name")]
    public string? AzureContainerName { get; set; }

    [Display(Name = "Base URL")]
    public string? WebDavBaseUrl { get; set; }

    [Display(Name = "Use Basic Auth")]
    public bool WebDavUseBasicAuth { get; set; }

    [Display(Name = "Username")]
    public string? WebDavUsername { get; set; }

    [Display(Name = "Password")]
    public string? WebDavPassword { get; set; }
}
