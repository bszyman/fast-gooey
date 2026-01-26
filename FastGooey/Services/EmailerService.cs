using System.Net;
using System.Net.Mail;
using FastGooey.Models.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace FastGooey.Services;

public class EmailerService: IEmailSender
{
    private readonly SmtpConfigurationModel _smtpSettings;
         
    public EmailerService(IOptions<SmtpConfigurationModel> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }
    
    public async Task SendEmailAsync(string recipientEmailAddress, string subject, string htmlMessage)
    {
        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
        };
             
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(recipientEmailAddress);
             
        await client.SendMailAsync(mailMessage);
    }
}