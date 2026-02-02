using FastGooey.Utils;

namespace FastGooey.Tests.Utils;

public class GuidShortIdTests
{
    [Fact]
    public void ToBase64Url_ReturnsExpectedLength()
    {
        var value = Guid.NewGuid();

        var shortId = value.ToBase64Url();

        Assert.Equal(22, shortId.Length);
        Assert.DoesNotContain("=", shortId);
    }

    [Fact]
    public void TryParse_RoundTripsBase64Url()
    {
        var value = Guid.NewGuid();
        var shortId = value.ToBase64Url();

        var result = GuidShortId.TryParse(shortId, out var parsed);

        Assert.True(result);
        Assert.Equal(value, parsed);
    }

    [Fact]
    public void TryParse_AcceptsGuidString()
    {
        var value = Guid.NewGuid();
        var guidString = value.ToString();

        var result = GuidShortId.TryParse(guidString, out var parsed);

        Assert.True(result);
        Assert.Equal(value, parsed);
    }

    [Fact]
    public void TryParse_RejectsInvalidValue()
    {
        var result = GuidShortId.TryParse("not-a-guid", out var parsed);

        Assert.False(result);
        Assert.Equal(Guid.Empty, parsed);
    }

    [Fact]
    public void NullableToBase64Url_ReturnsEmptyForNull()
    {
        Guid? value = null;

        var shortId = value.ToBase64Url();

        Assert.Equal(string.Empty, shortId);
    }

    [Theory]
    [InlineData("!!!!")] // Invalid Base64 characters
    [InlineData("abc")]  // Invalid length for Base64Url (must be multiple of 4 or 2/3 with padding)
    public void TryParse_HandlesInvalidBase64(string invalid)
    {
        var result = GuidShortId.TryParse(invalid, out var parsed);
        Assert.False(result);
        Assert.Equal(Guid.Empty, parsed);
    }
}
