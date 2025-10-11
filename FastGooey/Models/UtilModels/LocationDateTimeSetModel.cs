namespace FastGooey.Models.UtilModels;

public class LocationDateTimeSetModel
{
    public string LocalDate { get; set; } = string.Empty;
    public string LocalTime { get; set; } = string.Empty;
    public string LocalTimezone { get; set; } = string.Empty;

    public string Formatted()
    {
        return $"{LocalDate} {LocalTime} (UTC{LocalTimezone})";
    }
}