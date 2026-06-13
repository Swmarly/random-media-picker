using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Services;
public sealed class RandomMediaSelector(Random random)
{
    public MediaFile? Select(IReadOnlyList<MediaFile> files, AppSettings settings, RecentHistoryService history, string? previous)
    {
        if (files.Count == 0) return null;
        var eligible = files.Where(f => !((settings.AvoidImmediateRepeat && previous is not null && string.Equals(previous, f.Path, StringComparison.OrdinalIgnoreCase)) || (settings.AvoidRecentlyOpenedFiles && history.Items.Contains(f.Path, StringComparer.OrdinalIgnoreCase)))).ToList();
        if (eligible.Count == 0) eligible = files.Where(f => !(settings.AvoidImmediateRepeat && previous is not null && string.Equals(previous, f.Path, StringComparison.OrdinalIgnoreCase))).ToList();
        if (eligible.Count == 0) eligible = files.ToList();
        return eligible[random.Next(eligible.Count)];
    }
}
