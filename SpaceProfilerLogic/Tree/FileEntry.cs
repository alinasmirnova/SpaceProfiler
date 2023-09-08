namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, FileSystemEntry? parent) : base(fullName, parent)
    {
    }
    
    public FileEntry(string fullName, long size) : base(fullName)
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