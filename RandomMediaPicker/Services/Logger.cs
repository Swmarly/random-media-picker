namespace RandomMediaPicker.Services;
public sealed class Logger
{
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RandomMediaPicker", "app.log");
    public void Info(string message) => Write("INFO", message);
    public void Error(Exception ex, string message) => Write("ERROR", message + " " + ex);
    private void Write(string level, string message) { Directory.CreateDirectory(Path.GetDirectoryName(_path)!); File.AppendAllText(_path, $"{DateTimeOffset.Now:u} {level} {message}{Environment.NewLine}"); }
}
