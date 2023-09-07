namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, string? name, FileSystemEntry? parent) : base(fullName, name, parent)
    {
    }
    
    public FileEntry(string fullName, string? name, long size) : base(fullName, name)
    {
        Size = size;
    }

    public void SetSize(long size)
    {
        Size = size;
        var parent = Parent;
        while (parent != null)
        {
            parent.Size += size;
            parent = parent.Parent;
        }
    }
}