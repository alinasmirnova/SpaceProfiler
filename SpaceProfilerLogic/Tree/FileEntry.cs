namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, string? name, long size = 0) : base(fullName, name, size)
    {
    }
}