namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string Name { get; }

    protected long Size;
    public long GetSize => Size;

    public FileSystemEntry? Parent { get; set; }

    protected FileSystemEntry(string fullName, FileSystemEntry? parent = null)
    {
        FullName = fullName;
        Name = Path.GetFileName(fullName);
        Parent = parent;
    }
    
    public bool AddSize(long diff)
    {
        if (diff == 0)
            return false;

        Size  += diff;
        var parent = Parent;
        while (parent != null)
        {
            Interlocked.Add(ref parent.Size, diff);
            parent = parent.Parent;
        }

        return true;
    }

    public override string ToString()
    {
        return Name;
    }
}