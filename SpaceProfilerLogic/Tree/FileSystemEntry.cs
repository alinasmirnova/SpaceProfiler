namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string? Name { get; }

    public long Size { get; protected internal set; }

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