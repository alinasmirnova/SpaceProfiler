namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string Name { get; }

    protected internal long size;
    public long Size => size;

    public FileSystemEntry? Parent { get; set; }

    protected FileSystemEntry(string fullName, string? name, FileSystemEntry? parent = null)
    {
        FullName = fullName;
        Name = name ?? fullName;
        Parent = parent;
    }

    public override string ToString()
    {
        return Name ?? "null";
    }
}