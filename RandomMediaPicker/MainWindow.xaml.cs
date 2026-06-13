using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Windowing;
using RandomMediaPicker.Core.Models;
using RandomMediaPicker.Core.Persistence;
using RandomMediaPicker.Core.Services;
using RandomMediaPicker.Services;
using WinRT.Interop;
namespace RandomMediaPicker;
public sealed partial class MainWindow : Window
{
    private readonly Logger _log = new();
    private readonly JsonStateStore _store;
    private readonly FavoriteService _favorites = new();
    private readonly RecentHistoryService _history = new();
    private readonly FolderScanner _scanner = new();
    private readonly MediaScanCache _cache = new();
    private readonly RandomMediaSelector _selector = new(Random.Shared);
    private readonly ShellMediaOpener _shell = new();
    private AppSettings _settings = new();
    private CancellationTokenSource? _scanCts;
    private string? _currentMedia;
    private bool _loading;
    public MainWindow()
    {
        InitializeComponent();
        _store = new JsonStateStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RandomMediaPicker", "state.json"));
        _ = LoadAsync(); Closed += async (_, _) => await SaveAsync();
    }
    private async Task LoadAsync()
    {
        var state = await _store.LoadAsync(); _settings = state.Settings; _favorites.Favorites.AddRange(state.Favorites); _favorites.RefreshAvailability(); _history.Items = state.RecentHistory;
        if (_settings.RememberLastSelectedFolder) FolderPathBox.Text = _settings.LastSelectedFolder ?? string.Empty;
        FavoritesBox.ItemsSource = _favorites.Favorites; AppWindow.Resize(new Windows.Graphics.SizeInt32((int)_settings.WindowWidth, (int)_settings.WindowHeight));
    }
    private async Task SaveAsync()
    {
        _settings.WindowWidth = AppWindow.Size.Width; _settings.WindowHeight = AppWindow.Size.Height;
        if (_settings.RememberLastSelectedFolder) _settings.LastSelectedFolder = FolderPathBox.Text;
        await _store.SaveAsync(new AppState { Settings = _settings, Favorites = _favorites.Favorites, RecentHistory = _settings.AvoidRecentlyOpenedFiles ? _history.Items : [] });
    }
    private async void Browse_Click(object sender, RoutedEventArgs e)
    {
        var path = await new FolderPickerService(WindowNative.GetWindowHandle(this)).PickFolderAsync(); if (path is not null) { FolderPathBox.Text = path; _cache.Invalidate(); await SaveAsync(); }
    }
    private async void OpenRandom_Click(object sender, RoutedEventArgs e)
    {
        if (_loading) return; if (string.IsNullOrWhiteSpace(FolderPathBox.Text) || !Directory.Exists(FolderPathBox.Text)) { Show("Folder unavailable", "Choose an existing folder.", InfoBarSeverity.Error); return; }
        if (!_settings.IncludeImages && !_settings.IncludeVideos) { Show("Invalid settings", "Enable images or videos before scanning.", InfoBarSeverity.Warning); return; }
        _loading = true; OpenButton.IsEnabled = false; CancelButton.IsEnabled = true; BusyRing.IsActive = true; _scanCts = new(); ProgressText.Text = "Scanning…";
        try
        {
            var files = await GetCachedOrScannedFilesAsync(_scanCts.Token);
            var selected = SelectExistingFile(files);
            if (selected is null) { Show("No media found", "No eligible image or video files were found with the current settings. Use Rescan folder if files were added recently.", InfoBarSeverity.Warning); return; }
            _currentMedia = selected.Path; _history.Add(selected.Path, _settings.RecentHistorySize); await SaveAsync();
            if (_settings.OpeningMode == OpeningMode.WindowsDefaultApplication) { if (!await _shell.OpenAsync(selected.Path)) Show("Open failed", "Windows could not open this file with a default application.", InfoBarSeverity.Error); }
            else await ShowInternalAsync(selected);
            Show("Opened media", selected.Path, InfoBarSeverity.Success);
        }
        catch (OperationCanceledException) { Show("Scan canceled", "The random media scan was canceled.", InfoBarSeverity.Informational); }
        catch (Exception ex) { _log.Error(ex, "Random selection failed"); Show("Error", ex.Message, InfoBarSeverity.Error); }
        finally { _loading = false; OpenButton.IsEnabled = true; CancelButton.IsEnabled = false; BusyRing.IsActive = false; ProgressText.Text = string.Empty; _scanCts?.Dispose(); _scanCts = null; }
    }
    private async Task<IReadOnlyList<MediaFile>> GetCachedOrScannedFilesAsync(CancellationToken token)
    {
        if (_cache.HasUsableEntry(FolderPathBox.Text, _settings))
        {
            _cache.RemoveMissingFiles();
            ProgressText.Text = $"Using cached index ({_cache.GetFiles().Count:N0} media files)…";
            return _cache.GetFiles();
        }
        ProgressText.Text = "Indexing folder…";
        var files = await _scanner.ScanAsync(FolderPathBox.Text, _settings, new Progress<int>(c => ProgressText.Text = $"Checked {c:N0} files…"), token);
        _cache.Store(FolderPathBox.Text, _settings, files);
        ProgressText.Text = $"Cached {_cache.GetFiles().Count:N0} media files.";
        return _cache.GetFiles();
    }
    private MediaFile? SelectExistingFile(IReadOnlyList<MediaFile> files)
    {
        while (files.Count > 0)
        {
            var selected = _selector.Select(files, _settings, _history, _currentMedia);
            if (selected is null) return null;
            if (File.Exists(selected.Path)) return selected;
            _cache.RemoveMissingFiles();
            files = _cache.GetFiles();
        }
        return null;
    }
    private async Task ShowInternalAsync(MediaFile file)
    {
        MediaTitle.Text = file.FileName; MediaPathText.Text = file.Path; ImageViewer.Visibility = Visibility.Collapsed; VideoViewer.Visibility = Visibility.Collapsed; VideoViewer.Source = null;
        if (file.Kind == MediaKind.Image) { ImageViewer.Source = new BitmapImage(new Uri(file.Path)); ImageViewer.Visibility = Visibility.Visible; }
        else { VideoViewer.Source = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(file.Path)); VideoViewer.Visibility = Visibility.Visible; }
        await Task.CompletedTask;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => _scanCts?.Cancel();
    private void Rescan_Click(object sender, RoutedEventArgs e) { _cache.Invalidate(); Show("Index cleared", "The next random-media operation will rescan the selected folder.", InfoBarSeverity.Informational); }
    private async void AddFavorite_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(FolderPathBox.Text) && _favorites.Add(FolderPathBox.Text)) { FavoritesBox.ItemsSource = null; FavoritesBox.ItemsSource = _favorites.Favorites; await SaveAsync(); } else Show("Duplicate favorite", "That folder is already in favorites or no folder is selected.", InfoBarSeverity.Warning); }
    private void FavoritesBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (FavoritesBox.SelectedItem is FavoriteFolder f) { FolderPathBox.Text = f.Path; _cache.Invalidate(); if (!Directory.Exists(f.Path)) Show("Favorite unavailable", "This favorite no longer exists or cannot be accessed.", InfoBarSeverity.Warning); } }
    private async void RemoveFavorite_Click(object sender, RoutedEventArgs e) { if (FavoritesBox.SelectedItem is FavoriteFolder f) { _favorites.Remove(f); FavoritesBox.ItemsSource = null; FavoritesBox.ItemsSource = _favorites.Favorites; await SaveAsync(); } }
    private async void RenameFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesBox.SelectedItem is not FavoriteFolder f) return; var box = new TextBox { Text = f.DisplayName, Header = "Favorite display name" };
        var dialog = new ContentDialog { Title = "Rename favorite", Content = box, PrimaryButtonText = "Save", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(box.Text)) { f.DisplayName = box.Text.Trim(); FavoritesBox.ItemsSource = null; FavoritesBox.ItemsSource = _favorites.Favorites; await SaveAsync(); }
    }
    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        var panel = new Views.SettingsView(_settings, _history); var dialog = new ContentDialog { Title = "Settings", Content = panel, PrimaryButtonText = "Save", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) { panel.Apply(); _cache.Invalidate(); await SaveAsync(); }
    }
    private async void Reveal_Click(object sender, RoutedEventArgs e) { if (_currentMedia is not null && !await _shell.RevealAsync(_currentMedia)) Show("Reveal failed", "File Explorer could not reveal this file.", InfoBarSeverity.Error); }
    private async void OpenExternal_Click(object sender, RoutedEventArgs e) { if (_currentMedia is not null && !await _shell.OpenAsync(_currentMedia)) Show("Open failed", "Windows could not open this file with a default application.", InfoBarSeverity.Error); }
    private void Show(string title, string message, InfoBarSeverity severity) { StatusBar.Title = title; StatusBar.Message = message; StatusBar.Severity = severity; StatusBar.IsOpen = true; }
}
