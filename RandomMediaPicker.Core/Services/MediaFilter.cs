using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Services;
public sealed class MediaFilter
{
    private readonly HashSet<string> _images;
    private readonly HashSet<string> _videos;
    private readonly bool _includeImages;
    private readonly bool _includeVideos;
    public MediaFilter(AppSettings settings)
    {
        settings.Validate();
        _includeImages = settings.IncludeImages; _includeVideos = settings.IncludeVideos;
        _images = new(settings.ImageExtensions, StringComparer.OrdinalIgnoreCase);
        _videos = new(settings.VideoExtensions, StringComparer.OrdinalIgnoreCase);
    }
    public MediaKind? GetKind(string path)
    {
        var ext = ExtensionCatalog.Normalize(System.IO.Path.GetExtension(path));
        if (ext is null) return null;
        if (_includeImages && _images.Contains(ext)) return MediaKind.Image;
        if (_includeVideos && _videos.Contains(ext)) return MediaKind.Video;
        return null;
    }
}
