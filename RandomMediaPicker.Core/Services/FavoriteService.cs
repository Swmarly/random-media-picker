using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Services;
public sealed class FavoriteService
{
    public List<FavoriteFolder> Favorites { get; } = [];
    public bool Add(string path, string? name = null)
    {
        var normalized = NormalizePath(path);
        if (Favorites.Any(f => string.Equals(NormalizePath(f.Path), normalized, StringComparison.OrdinalIgnoreCase))) return false;
        Favorites.Add(new FavoriteFolder { Path = normalized, DisplayName = string.IsNullOrWhiteSpace(name) ? System.IO.Path.GetFileName(normalized.TrimEnd(System.IO.Path.DirectorySeparatorChar)) : name.Trim(), IsAvailable = Directory.Exists(normalized) });
        return true;
    }
    public void Remove(FavoriteFolder favorite) => Favorites.Remove(favorite);
    public void RefreshAvailability() { foreach (var f in Favorites) f.IsAvailable = Directory.Exists(f.Path); }
    public static string NormalizePath(string path) => System.IO.Path.GetFullPath(path.Trim()).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
}
