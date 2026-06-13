namespace RandomMediaPicker.Core.Services;
public sealed class RecentHistoryService
{
    public List<string> Items { get; set; } = [];
    public bool IsExcluded(string path, bool avoidRecent, string? previous) => (previous is not null && string.Equals(previous, path, StringComparison.OrdinalIgnoreCase)) || (avoidRecent && Items.Contains(path, StringComparer.OrdinalIgnoreCase));
    public void Add(string path, int max)
    {
        Items.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        Items.Insert(0, path);
        if (Items.Count > max) Items.RemoveRange(max, Items.Count - max);
    }
    public void Clear() => Items.Clear();
}
