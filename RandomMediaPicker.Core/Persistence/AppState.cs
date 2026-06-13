using RandomMediaPicker.Core.Models;
namespace RandomMediaPicker.Core.Persistence;
public sealed class AppState
{
    public AppSettings Settings { get; set; } = new();
    public List<FavoriteFolder> Favorites { get; set; } = [];
    public List<string> RecentHistory { get; set; } = [];
}
