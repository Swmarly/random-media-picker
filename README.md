# Random Media Picker

Random Media Picker is a packaged WinUI 3 desktop application for Windows. Choose a folder, then press **Open Random Media** to scan that folder (and optionally subfolders) and open one eligible image or video with equal probability.

## Features

- WinUI 3 and C#/.NET 8 desktop application packaged as MSIX.
- Folder picker, current-folder display, favorite folders with rename/remove and duplicate detection.
- Asynchronous, cancellable recursive scanning that skips directory reparse points to avoid recursion loops.
- In-memory media-index caching so pressing **Open Random Media** repeatedly does not rescan the same folder/settings combination; use **Rescan folder** after adding or removing media.
- Configurable image/video extension lists with normalization for casing and leading periods.
- Internal viewer for images, animated GIFs, and videos; external opening through Windows file associations.
- Persistent settings, favorites, last folder, recent media history, and window size.
- Unit tests for extension normalization, filtering, favorites, random selection, recent-history behavior, and JSON fallback.

## Requirements

- Windows 10 version 1809 or later; Windows 11 recommended.
- Visual Studio 2022 17.10 or later with the **Windows application development** workload and .NET desktop tools.
- Windows App SDK stable runtime matching the NuGet package used by the project. This project references `Microsoft.WindowsAppSDK` 2.2.0, selected from the current stable Windows App SDK/NuGet information available when generated.
- .NET 8 SDK.

## Dependencies

- `Microsoft.WindowsAppSDK`: WinUI 3, Windows App SDK packaging, and Windows desktop APIs.
- `CommunityToolkit.Mvvm`: included for maintainable MVVM-friendly patterns as the application evolves.
- `xunit`, `Microsoft.NET.Test.Sdk`, and `coverlet.collector`: unit testing and coverage collection.

## Restore, build, test, and run

```powershell
dotnet restore .\RandomMediaPicker.sln
dotnet test .\RandomMediaPicker.sln
```

To build and run the packaged app, open `RandomMediaPicker.sln` in Visual Studio, select the `RandomMediaPicker` startup project, select `x64`, then press **F5**. You can also use **Build > Build Solution** and deploy/run the MSIX project from Visual Studio.

## Usage

1. Click **Browse…** and choose a folder.
2. Optionally click **Add favorite** to persist the folder.
3. Click **Open Random Media**.
4. Use **Settings** to choose internal viewing or Windows default application opening, enable/disable images or videos, edit extensions, and configure repeat avoidance.
5. Use **Cancel scan** to stop a long-running folder scan.
6. Use **Rescan folder** to clear the in-memory index when folder contents have changed.

## Persistence

State is stored as JSON in `%LOCALAPPDATA%\RandomMediaPicker\state.json`. The media index cache is intentionally in memory only, so saved state does not retain stale file lists across app restarts. Recoverable failures and diagnostic information are written to `%LOCALAPPDATA%\RandomMediaPicker\app.log`. If the JSON file is missing, malformed, or outdated, the application falls back to safe defaults.

## Media format notes

The application filters using broad image and video extension lists, including JPEG, PNG, BMP, WebP, TIFF, GIF, MP4, WebM, AVI, MOV, MKV, WMV, and M4V. Actual decoding depends on Windows, installed codecs, and WinUI media controls. Files with unsupported codecs are reported through normal media/opening error paths rather than being loaded into memory or executed.
