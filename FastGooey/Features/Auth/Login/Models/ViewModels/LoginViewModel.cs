using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Features.Auth.Login.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty(Name = "cf-turnstile-response")]
    public string? TurnstileToken { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}