namespace FastGooey.Services.Media;

public class MediaStreamResult
{
    public MediaStreamResult(Stream stream, string? contentType, long? contentLength)
    {
        Stream = stream;
        ContentType = contentType;
        ContentLength = contentLength;
    }

    public Stream Stream { get; }
    public string? ContentType { get; }
    public long? ContentLength { get; }
}
