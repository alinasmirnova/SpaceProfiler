namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string? Name { get; }

    private long size;
    public long Size
    {
        get => size;
        set
        {
            if (Parent != null)
                Parent.Size += value - size;
            size = value;
        } 
    }

    public FileSystemEntry? Parent { get; set; }

    protected FileSystemEntry(string fullName, string? name, FileSystemEntry? parent = null)
    {
        FullName = fullName;
        Name = name;
        Parent = parent;
    }

    public override string ToString()
    {
        return Name ?? "null";
    }
}