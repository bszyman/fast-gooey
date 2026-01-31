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
}
