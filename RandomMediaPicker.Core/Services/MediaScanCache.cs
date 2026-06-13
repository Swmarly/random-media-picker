using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Services;
public sealed class MediaScanCache
{
    private string? _key;
    private List<MediaFile> _files = [];
    public bool HasUsableEntry(string folder, AppSettings settings) => string.Equals(_key, CreateKey(folder, settings), StringComparison.Ordinal);
    public IReadOnlyList<MediaFile> GetFiles() => _files;
    public void Store(string folder, AppSettings settings, IEnumerable<MediaFile> files)
    {
        _key = CreateKey(folder, settings);
        _files = files.Where(f => File.Exists(f.Path)).ToList();
    }
    public void RemoveMissingFiles() => _files.RemoveAll(f => !File.Exists(f.Path));
    public void Invalidate() { _key = null; _files.Clear(); }
    public static string CreateKey(string folder, AppSettings settings)
    {
        settings.Validate();
        return string.Join('|', FavoriteService.NormalizePath(folder), settings.IncludeSubfolders, settings.IncludeImages, settings.IncludeVideos, string.Join(',', settings.ImageExtensions.Order(StringComparer.OrdinalIgnoreCase)), string.Join(',', settings.VideoExtensions.Order(StringComparer.OrdinalIgnoreCase)));
    }
}
