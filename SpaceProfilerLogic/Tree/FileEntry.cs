namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, string? name, long size = 0, FileSystemEntry? parent = null) : base(fullName, name, size, parent)
    {
    }
}