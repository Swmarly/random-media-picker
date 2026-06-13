using RandomMediaPicker.Core.Models;
using RandomMediaPicker.Core.Persistence;
using RandomMediaPicker.Core.Services;
using Xunit;
namespace RandomMediaPicker.Tests;
public sealed class CoreTests
{
    [Fact] public void ExtensionsNormalizeConsistently() => Assert.Equal([".jpg", ".png"], ExtensionCatalog.NormalizeMany(["JPG", ".jpg", " png "]));
    [Fact] public void MediaFilterHonorsKinds() { var f = new MediaFilter(new AppSettings { IncludeImages = true, IncludeVideos = false }); Assert.Equal(MediaKind.Image, f.GetKind("a.GIF")); Assert.Null(f.GetKind("a.mp4")); }
    [Fact] public void FavoritesAvoidDuplicatePaths() { var s = new FavoriteService(); Assert.True(s.Add("C:/Temp/")); Assert.False(s.Add("C:/Temp")); }
    [Fact] public void RandomSelectorAvoidsImmediateRepeatWhenPossible() { var files = new[] { new MediaFile("a.jpg", MediaKind.Image), new MediaFile("b.jpg", MediaKind.Image) }; var selected = new RandomMediaSelector(new Random(1)).Select(files, new AppSettings(), new RecentHistoryService(), "a.jpg"); Assert.NotEqual("a.jpg", selected!.Path); }
    [Fact] public void RecentHistoryTrimsAndExcludes() { var h = new RecentHistoryService(); h.Add("a", 2); h.Add("b", 2); h.Add("c", 2); Assert.DoesNotContain("a", h.Items); Assert.True(h.IsExcluded("c", true, null)); }
    [Fact] public void MediaScanCacheReusesSameSettingsAndInvalidates() { var settings = new AppSettings(); var cache = new MediaScanCache(); cache.Store("C:/Media", settings, [new MediaFile("missing.jpg", MediaKind.Image)]); Assert.True(cache.HasUsableEntry("C:/Media/", settings)); settings.IncludeVideos = false; Assert.False(cache.HasUsableEntry("C:/Media", settings)); cache.Invalidate(); Assert.Empty(cache.GetFiles()); }
    [Fact] public async Task StateStoreFallsBackForMalformedJson() { var p = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".json"); await File.WriteAllTextAsync(p, "not json"); var state = await new JsonStateStore(p).LoadAsync(); Assert.True(state.Settings.IncludeImages); }
}
