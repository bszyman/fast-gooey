using System.Net;
using System.Net.Mail;
using FastGooey.Models;
using FastGooey.Models.Configuration;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace FastGooey.Services;

public class EmailerService : IEmailSender
{
    private readonly SmtpConfigurationModel _smtpSettings;

    public EmailerService(IOptions<SmtpConfigurationModel> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string recipientEmailAddress, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(recipientEmailAddress))
        {
            return;
        }

        var fromAddress = string.IsNullOrWhiteSpace(_smtpSettings.FromEmail)
            ? (_smtpSettings.Username ?? "no-reply@localhost")
            : _smtpSettings.FromEmail;
        var fromName = string.IsNullOrWhiteSpace(_smtpSettings.FromName)
            ? "FastGooey"
            : _smtpSettings.FromName;

        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(recipientEmailAddress);

        await client.SendMailAsync(mailMessage);
    }

    public async Task SendVerificationEmail(ApplicationUser user, string confirmationLink)
    {
        var recipientEmail = user.Email;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return;
        }

        var name = $"{user.FirstName} {user.LastName}";

        const string filePath = "Views/EmailNotifications/emailVerification.handlebars";
        var fileContents = await System.IO.File.ReadAllTextAsync(filePath);

        var template = Handlebars.Compile(fileContents);

        var data = new
        {
            name,
            confirmationLink
        };

        var messageContents = template(data);

        await SendEmailAsync(
            recipientEmail,
            "Welcome to FastGooey! Please verify your email address.",
            messageContents
        );
    }

    public async Task SendStripeWelcomeEmail(ApplicationUser user, string temporaryPassword)
    {
        var recipientEmail = user.Email;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return;
        }

        var name = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = user.Email ?? "there";
        }

        const string filePath = "Views/EmailNotifications/stripeWelcome.handlebars";
        var fileContents = await System.IO.File.ReadAllTextAsync(filePath);

        var template = Handlebars.Compile(fileContents);

        var data = new
        {
            name,
            temporaryPassword
        };

        var messageContents = template(data);

        await SendEmailAsync(
            recipientEmail,
            "Welcome to FastGooey! Your account is ready.",
            messageContents
        );
    }

    public async Task SendMagicLinkEmail(ApplicationUser user, string magicLink)
    {
        var recipientEmail = user.Email;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return;
        }

        var name = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = user.Email ?? "there";
        }

        const string filePath = "Views/EmailNotifications/magicLink.handlebars";
        var fileContents = await System.IO.File.ReadAllTextAsync(filePath);

        var template = Handlebars.Compile(fileContents);

        var data = new
        {
            name,
            magicLink
        };

        var messageContents = template(data);

        await SendEmailAsync(
            recipientEmail,
            "Your FastGooey sign-in link",
            messageContents
        );
    }
}
