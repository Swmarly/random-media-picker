using Microsoft.UI.Xaml;
using Windows.System;
namespace RandomMediaPicker.Services;
public sealed class ShellMediaOpener
{
    public async Task<bool> OpenAsync(string path) => File.Exists(path) && await Launcher.LaunchFileAsync(await Windows.Storage.StorageFile.GetFileFromPathAsync(path));
    public async Task<bool> RevealAsync(string path)
    {
        if (!File.Exists(path)) return false;
        var folder = Path.GetDirectoryName(path); if (folder is null) return false;
        return await Launcher.LaunchFolderPathAsync(folder, new FolderLauncherOptions { ItemsToSelect = { await Windows.Storage.StorageFile.GetFileFromPathAsync(path) } });
    }
}
