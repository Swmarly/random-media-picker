using WinRT.Interop;
using Windows.Storage.Pickers;
namespace RandomMediaPicker.Services;
public sealed class FolderPickerService(IntPtr hwnd)
{
    public async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
        picker.FileTypeFilter.Add("*"); InitializeWithWindow.Initialize(picker, hwnd);
        var folder = await picker.PickSingleFolderAsync(); return folder?.Path;
    }
}
