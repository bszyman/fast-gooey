using System.Text.RegularExpressions;
using FastGooey.Utils;

namespace FastGooey.Tests.Utils;

public class TimeFromCoordinatesTests
{
    [Fact]
    public void CalculateDateTimeSet_ReturnsPopulatedFields()
    {
        var result = TimeFromCoordinates.CalculateDateTimeSet(37.7749, -122.4194);

        Assert.False(string.IsNullOrWhiteSpace(result.LocalDate));
        Assert.False(string.IsNullOrWhiteSpace(result.LocalTime));
        Assert.False(string.IsNullOrWhiteSpace(result.LocalTimezone));
    }

    [Fact]
    public void CalculateDateTimeSet_ParsesStringCoordinates()
    {
        var result = TimeFromCoordinates.CalculateDateTimeSet("37.7749", "-122.4194");

        Assert.Matches(new Regex(@"^\w+ \d{1,2}, \d{4}$"), result.LocalDate);
        Assert.Matches(new Regex(@"^\d{1,2}:\d{2} [AP]M$"), result.LocalTime);
        Assert.False(string.IsNullOrWhiteSpace(result.LocalTimezone));
    }
}
