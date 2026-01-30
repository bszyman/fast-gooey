using FastGooey.Models.UtilModels;
using GeoTimeZone;
using NodaTime;

namespace FastGooey.Utils;

public static class TimeFromCoordinates
{
    public static LocationDateTimeSetModel CalculateDateTimeSet(double latitude, double longitude)
    {
        var localTime = GetLocalTime(latitude, longitude);
        var localDate = GetLocalDate(latitude, longitude);
        var localTimezone = GetLocalTimezone(latitude, longitude);

        return new LocationDateTimeSetModel
        {
            LocalTime = localTime,
            LocalDate = localDate,
            LocalTimezone = localTimezone
        };
    }

    public static LocationDateTimeSetModel CalculateDateTimeSet(string latitude, string longitude)
    {
        var latitudeDouble = double.Parse(latitude);
        var longitudeDouble = double.Parse(longitude);

        var localTime = GetLocalTime(latitudeDouble, longitudeDouble);
        var localDate = GetLocalDate(latitudeDouble, longitudeDouble);
        var localTimezone = GetLocalTimezone(latitudeDouble, longitudeDouble);

        return new LocationDateTimeSetModel
        {
            LocalTime = localTime,
            LocalDate = localDate,
            LocalTimezone = localTimezone
        };
    }

    public static string GetLocalTime(double latitude, double longitude)
    {
        // Get IANA timezone ID from coordinates
        var tzId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

        // Convert to local time
        var tz = DateTimeZoneProviders.Tzdb[tzId];
        var instant = SystemClock.Instance.GetCurrentInstant();
        var zonedDateTime = instant.InZone(tz);

        return zonedDateTime.ToString("h:mm tt", null);
    }

    public static string GetLocalDate(double latitude, double longitude)
    {
        // Get IANA timezone ID from coordinates
        var tzId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

        // Convert to local time
        var tz = DateTimeZoneProviders.Tzdb[tzId];
        var instant = SystemClock.Instance.GetCurrentInstant();
        var zonedDateTime = instant.InZone(tz);

        return zonedDateTime.ToString("MMMM d, yyyy", null);
    }

    public static string GetLocalTimezone(double latitude, double longitude)
    {
        var tzId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
        var tz = DateTimeZoneProviders.Tzdb[tzId];
        var offset = tz.GetUtcOffset(SystemClock.Instance.GetCurrentInstant()).ToString();

        return offset;
    }
}