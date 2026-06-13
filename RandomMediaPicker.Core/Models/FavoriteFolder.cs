namespace RandomMediaPicker.Core.Models;
public sealed class FavoriteFolder
{
    public string DisplayName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
