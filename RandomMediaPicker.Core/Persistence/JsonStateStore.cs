using System.Text.Json;
namespace RandomMediaPicker.Core.Persistence;
public sealed class JsonStateStore(string filePath)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public async Task<AppState> LoadAsync()
    {
        try
        {
            if (!File.Exists(filePath)) return new AppState();
            await using var stream = File.OpenRead(filePath);
            var state = await JsonSerializer.DeserializeAsync<AppState>(stream, Options).ConfigureAwait(false) ?? new AppState();
            state.Settings.Validate(); return state;
        }
        catch { return new AppState(); }
    }
    public async Task SaveAsync(AppState state)
    {
        state.Settings.Validate(); Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var temp = filePath + ".tmp";
        await using (var stream = File.Create(temp)) await JsonSerializer.SerializeAsync(stream, state, Options).ConfigureAwait(false);
        File.Move(temp, filePath, true);
    }
}
