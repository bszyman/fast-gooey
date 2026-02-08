using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.Media;

public enum MediaSourceType
{
    [Display(Name = "Amazon S3")]
    S3 = 0,
    [Display(Name = "Azure Blob Storage")]
    AzureBlob = 1,
    [Display(Name = "WebDAV")]
    WebDav = 2
}
