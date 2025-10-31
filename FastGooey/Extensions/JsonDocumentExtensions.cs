using System.Text.Json;

namespace FastGooey.Extensions;

public static class JsonDocumentExtensions
{
    public static readonly JsonSerializerOptions PolymorphicOptions = new()
    {
        AllowOutOfOrderMetadataProperties = true,
        PropertyNameCaseInsensitive = true
    };

    public static T? DeserializePolymorphic<T>(this JsonDocument document)
    {
        return JsonSerializer.Deserialize<T>(document, PolymorphicOptions);
    }
}