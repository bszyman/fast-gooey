using FastGooey.Models.Media;

namespace FastGooey.Services.Media;

public interface IMediaSourceProviderRegistry
{
    IMediaSourceProvider GetProvider(MediaSourceType sourceType);
}

public class MediaSourceProviderRegistry : IMediaSourceProviderRegistry
{
    private readonly IReadOnlyDictionary<MediaSourceType, IMediaSourceProvider> _providers;

    public MediaSourceProviderRegistry(IEnumerable<IMediaSourceProvider> providers)
    {
        _providers = providers.ToDictionary(provider => provider.SourceType, provider => provider);
    }

    public IMediaSourceProvider GetProvider(MediaSourceType sourceType)
    {
        if (_providers.TryGetValue(sourceType, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"No media source provider registered for {sourceType}.");
    }
}
