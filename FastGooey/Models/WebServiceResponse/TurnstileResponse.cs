namespace FastGooey.Models.WebServiceResponse;

public class TurnstileResponse
{
    public bool Success { get; set; }
    public string[] ErrorCodes { get; set; } = [];
    public string Challenge_ts { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
}