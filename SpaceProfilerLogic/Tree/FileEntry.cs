namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, string? name, FileSystemEntry? parent = null) : base(fullName, name, parent)
    {
    }
    
    public FileEntry(string fullName, string? name, long size) : base(fullName, name)
    {
        Size = size;
    }
}