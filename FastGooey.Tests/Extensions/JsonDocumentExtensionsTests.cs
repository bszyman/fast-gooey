using System.Text.Json;
using FastGooey.Extensions;

namespace FastGooey.Tests.Extensions;

public class JsonDocumentExtensionsTests
{
    private sealed class SamplePayload
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    [Fact]
    public void DeserializePolymorphic_IsCaseInsensitive()
    {
        using var document = JsonDocument.Parse("{\"NAME\":\"Gooey\",\"count\":3}");

        var result = document.DeserializePolymorphic<SamplePayload>();

        Assert.NotNull(result);
        Assert.Equal("Gooey", result!.Name);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void HasImage_ReturnsTrue_WhenPreviewMediaUrlExists()
    {
        using var document = JsonDocument.Parse("{\"PreviewMediaUrl\":\"https://images.fastgooey.com/preview.png\"}");

        var hasImage = document.HasImage();

        Assert.True(hasImage);
    }

    [Fact]
    public void HasImage_ReturnsFalse_WhenImageFieldsAreMissing()
    {
        using var document = JsonDocument.Parse("{\"Title\":\"Featured\",\"Description\":\"A product\"}");

        var hasImage = document.HasImage();

        Assert.False(hasImage);
    }
}
