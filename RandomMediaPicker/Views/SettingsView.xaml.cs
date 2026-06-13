using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RandomMediaPicker.Core.Models;
using RandomMediaPicker.Core.Services;
namespace RandomMediaPicker.Views;
public sealed partial class SettingsView : UserControl
{
    private readonly AppSettings _settings; private readonly RecentHistoryService _history;
    public SettingsView(AppSettings settings, RecentHistoryService history)
    {
        InitializeComponent(); _settings = settings; _history = history;
        OpeningModeBox.SelectedIndex = settings.OpeningMode == OpeningMode.InternalViewer ? 0 : 1; IncludeSubfoldersBox.IsChecked = settings.IncludeSubfolders; IncludeImagesBox.IsChecked = settings.IncludeImages; IncludeVideosBox.IsChecked = settings.IncludeVideos;
        ImageExtensionsBox.Text = string.Join(", ", settings.ImageExtensions); VideoExtensionsBox.Text = string.Join(", ", settings.VideoExtensions); RememberFolderBox.IsChecked = settings.RememberLastSelectedFolder; AvoidRepeatBox.IsChecked = settings.AvoidImmediateRepeat; AvoidRecentBox.IsChecked = settings.AvoidRecentlyOpenedFiles; HistorySizeBox.Value = settings.RecentHistorySize;
    }
    public void Apply()
    {
        _settings.OpeningMode = OpeningModeBox.SelectedIndex == 0 ? OpeningMode.InternalViewer : OpeningMode.WindowsDefaultApplication; _settings.IncludeSubfolders = IncludeSubfoldersBox.IsChecked == true; _settings.IncludeImages = IncludeImagesBox.IsChecked == true; _settings.IncludeVideos = IncludeVideosBox.IsChecked == true;
        _settings.ImageExtensions = ExtensionCatalog.ParseList(ImageExtensionsBox.Text).ToList(); _settings.VideoExtensions = ExtensionCatalog.ParseList(VideoExtensionsBox.Text).ToList(); _settings.RememberLastSelectedFolder = RememberFolderBox.IsChecked == true; _settings.AvoidImmediateRepeat = AvoidRepeatBox.IsChecked == true; _settings.AvoidRecentlyOpenedFiles = AvoidRecentBox.IsChecked == true; _settings.RecentHistorySize = (int)Math.Clamp(HistorySizeBox.Value, 1, 500); _settings.Validate();
        WarningBar.IsOpen = !_settings.IncludeImages && !_settings.IncludeVideos;
    }
    private void ClearHistory_Click(object sender, RoutedEventArgs e) => _history.Clear();
}
