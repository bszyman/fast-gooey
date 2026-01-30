namespace FastGooey.Models.Configuration;

public class StripeConfigurationModel
{
    public string? SecretKey { get; set; }
    public Dictionary<string, string>? Prices { get; set; }
}