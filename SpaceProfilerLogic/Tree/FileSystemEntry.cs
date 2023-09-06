namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public string? Name { get; }
    public long Size { get; set; }
    public List<FileSystemEntry> Children { get; } = new();
    public FileSystemEntry(string fullName, string? name, long size = 0)
    {
        FullName = fullName;
        Name = name;
        Size = size;
    }
}