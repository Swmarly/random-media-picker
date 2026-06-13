namespace RandomMediaPicker.Core.Models;
public sealed class AppSettings
{
    public OpeningMode OpeningMode { get; set; } = OpeningMode.InternalViewer;
    public bool IncludeSubfolders { get; set; } = true;
    public bool IncludeImages { get; set; } = true;
    public bool IncludeVideos { get; set; } = true;
    public List<string> ImageExtensions { get; set; } = ExtensionCatalog.DefaultImageExtensions.ToList();
    public List<string> VideoExtensions { get; set; } = ExtensionCatalog.DefaultVideoExtensions.ToList();
    public bool RememberLastSelectedFolder { get; set; } = true;
    public bool AvoidImmediateRepeat { get; set; } = true;
    public bool AvoidRecentlyOpenedFiles { get; set; }
    public int RecentHistorySize { get; set; } = 20;
    public string? LastSelectedFolder { get; set; }
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public void Validate()
    {
        ImageExtensions = ExtensionCatalog.NormalizeMany(ImageExtensions).DefaultIfEmpty(".jpg").ToList();
        VideoExtensions = ExtensionCatalog.NormalizeMany(VideoExtensions).DefaultIfEmpty(".mp4").ToList();
        RecentHistorySize = Math.Clamp(RecentHistorySize, 1, 500);
    }
}
