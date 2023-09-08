namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string Name { get; }

    protected internal long size;
    public long Size => size;

    public FileSystemEntry? Parent { get; set; }

    protected FileSystemEntry(string fullName, FileSystemEntry? parent = null)
    {
        FullName = fullName;
        Name = Path.GetFileName(fullName);
        Parent = parent;
    }

    public override string ToString()
    {
        return Name;
    }
}