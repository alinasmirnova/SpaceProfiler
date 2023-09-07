namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, string? name, FileSystemEntry? parent) : base(fullName, name, parent)
    {
    }
    
    public FileEntry(string fullName, string? name, long size) : base(fullName, name)
    {
        this.size = size;
    }

    public void SetSize(long s)
    {
        size = s;
        var parent = Parent;
        while (parent != null)
        {
            Interlocked.Add(ref parent.size, s);
            parent = parent.Parent;
        }
    }
}