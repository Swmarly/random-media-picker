namespace RandomMediaPicker.Core.Models;
public sealed record MediaFile(string Path, MediaKind Kind)
{
    public string FileName => System.IO.Path.GetFileName(Path);
}
