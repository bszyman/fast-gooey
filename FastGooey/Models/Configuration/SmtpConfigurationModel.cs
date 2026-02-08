namespace FastGooey.Models.Configuration;

public class SmtpConfigurationModel
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}