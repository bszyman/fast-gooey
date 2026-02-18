using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Features.Auth.SignUp.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [BindProperty(Name = "cf-turnstile-response")]
    public string? TurnstileToken { get; set; }
}
