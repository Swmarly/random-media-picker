namespace RandomMediaPicker.Core.Models;
public static class ExtensionCatalog
{
    public static readonly string[] DefaultImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tif", ".tiff", ".gif", ".heic", ".heif", ".ico", ".jfif"];
    public static readonly string[] DefaultVideoExtensions = [".mp4", ".m4v", ".webm", ".avi", ".mov", ".mkv", ".wmv", ".mpg", ".mpeg", ".3gp", ".3g2", ".ts", ".m2ts"];
    public static string? Normalize(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return null;
        var value = extension.Trim().ToLowerInvariant();
        if (value == ".") return null;
        return value.StartsWith('.') ? value : "." + value;
    }
    public static IReadOnlyList<string> NormalizeMany(IEnumerable<string> values) => values.Select(Normalize).Where(v => v is not null).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    public static IReadOnlyList<string> ParseList(string text) => NormalizeMany(text.Split([',', ';', '\r', '\n', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
}
