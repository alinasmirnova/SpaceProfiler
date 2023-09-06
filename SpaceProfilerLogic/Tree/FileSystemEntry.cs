namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string? Name { get; }
    public long Size { get; set; }
    public FileSystemEntry? Parent { get; set; }
    
    public FileSystemEntry(string fullName, string? name, long size = 0, FileSystemEntry? parent = null)
    {
        FullName = fullName;
        Name = name;
        Parent = parent;
        Size = size;
    }
}