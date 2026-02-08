using FastGooey.Models.Media;

namespace FastGooey.Services.Media;

public interface IMediaSourceProvider
{
    MediaSourceType SourceType { get; }
    Task<IReadOnlyList<MediaItem>> ListAsync(MediaSource source, string? path, CancellationToken cancellationToken);
    Task<MediaStreamResult?> OpenReadAsync(MediaSource source, string path, CancellationToken cancellationToken);
}
