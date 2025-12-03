using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [BindProperty(Name="cf-turnstile-response")]
    public string? TurnstileToken { get; set; }
}