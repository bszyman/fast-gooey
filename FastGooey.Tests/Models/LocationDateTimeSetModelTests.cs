using FastGooey.Models.UtilModels;

namespace FastGooey.Tests.Models;

public class LocationDateTimeSetModelTests
{
    [Fact]
    public void Formatted_ReturnsCombinedString()
    {
        var model = new LocationDateTimeSetModel
        {
            LocalDate = "January 2, 2024",
            LocalTime = "1:23 PM",
            LocalTimezone = "-05:00"
        };

        var formatted = model.Formatted();

        Assert.Equal("January 2, 2024 1:23 PM (UTC-05:00)", formatted);
    }
}
