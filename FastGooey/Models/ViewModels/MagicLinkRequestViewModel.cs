using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Models.ViewModels;

public class MagicLinkRequestViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty(Name = "cf-turnstile-response")]
    public string? TurnstileToken { get; set; }
}
