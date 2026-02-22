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

    public static bool HasImage(this JsonDocument? document)
    {
        return document is not null && HasImage(document.RootElement);
    }

    private static bool HasImage(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String &&
                        IsImagePropertyName(property.Name) &&
                        !string.IsNullOrWhiteSpace(property.Value.GetString()))
                    {
                        return true;
                    }

                    if (property.Value.ValueKind == JsonValueKind.Object ||
                        property.Value.ValueKind == JsonValueKind.Array)
                    {
                        if (HasImage(property.Value))
                        {
                            return true;
                        }
                    }
                }

                return false;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (HasImage(item))
                    {
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }

    private static bool IsImagePropertyName(string name)
    {
        return name.Contains("image", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("mediaurl", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("previewmedia", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("poster", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("thumbnail", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("heroimg", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("img", StringComparison.OrdinalIgnoreCase);
    }
}
