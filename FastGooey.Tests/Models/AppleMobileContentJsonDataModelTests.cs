using System.Text.Json;
using FastGooey.Extensions;
using FastGooey.Models.JsonDataModels;
using Xunit;

namespace FastGooey.Tests.Models;

public class AppleMobileContentJsonDataModelTests
{
    [Fact]
    public void Deserialization_HandlesPolymorphicItems()
    {
        // Arrange
        var json = """
        {
          "HeaderTitle": "My Mobile App",
          "Items": [
            { "$type": "headline", "Headline": "Breaking News" },
            { "$type": "text", "Text": "Some long text content here." },
            { "$type": "link", "Url": "https://example.com", "Title": "Visit Us" },
            { "$type": "image", "Url": "https://example.com/img.png", "AltText": "Cool Image" },
            { "$type": "video", "Url": "https://example.com/vid.mp4", "ThumbnailUrl": "https://example.com/thumb.png" }
          ]
        }
        """;

        using var document = JsonDocument.Parse(json);

        // Act
        var result = document.DeserializePolymorphic<AppleMobileContentJsonDataModel>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My Mobile App", result.HeaderTitle);
        Assert.Equal(5, result.Items.Count);
        
        Assert.IsType<HeadlineContentItem>(result.Items[0]);
        Assert.Equal("Breaking News", ((HeadlineContentItem)result.Items[0]).Headline);
        
        Assert.IsType<TextContentItem>(result.Items[1]);
        Assert.Equal("Some long text content here.", ((TextContentItem)result.Items[1]).Text);
        
        Assert.IsType<LinkContentItem>(result.Items[2]);
        Assert.Equal("https://example.com", ((LinkContentItem)result.Items[2]).Url);
        
        Assert.IsType<ImageContentItem>(result.Items[3]);
        Assert.Equal("https://example.com/img.png", ((ImageContentItem)result.Items[3]).Url);
        
        Assert.IsType<VideoContentItem>(result.Items[4]);
        Assert.Equal("https://example.com/vid.mp4", ((VideoContentItem)result.Items[4]).Url);
    }
}
