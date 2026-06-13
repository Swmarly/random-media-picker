using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Services;
public sealed class FolderScanner
{
    public Task<IReadOnlyList<MediaFile>> ScanAsync(string root, AppSettings settings, IProgress<int>? progress, CancellationToken token) => Task.Run(() => Scan(root, settings, progress, token), token);
    private static IReadOnlyList<MediaFile> Scan(string root, AppSettings settings, IProgress<int>? progress, CancellationToken token)
    {
        var result = new List<MediaFile>();
        if (!Directory.Exists(root) || (!settings.IncludeImages && !settings.IncludeVideos)) return result;
        var filter = new MediaFilter(settings); var dirs = new Stack<string>(); dirs.Push(root); var seen = 0;
        while (dirs.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            var dir = dirs.Pop();
            try
            {
                foreach (var file in Directory.EnumerateFiles(dir))
                {
                    token.ThrowIfCancellationRequested();
                    try { var attrs = File.GetAttributes(file); if ((attrs & (FileAttributes.Temporary | FileAttributes.System)) != 0) continue; } catch { continue; }
                    var kind = filter.GetKind(file); if (kind is not null) result.Add(new MediaFile(file, kind.Value));
                    if (++seen % 100 == 0) progress?.Report(seen);
                }
                if (settings.IncludeSubfolders)
                    foreach (var sub in Directory.EnumerateDirectories(dir))
                    { try { if ((File.GetAttributes(sub) & FileAttributes.ReparsePoint) == 0) dirs.Push(sub); } catch { } }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }
        progress?.Report(seen); return result;
    }
}
