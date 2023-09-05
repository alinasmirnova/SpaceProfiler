namespace SpaceProfilerLogic.Tree;

public class FileSystemEntry
{
    public string FullName { get; }
    public long Size { get; set; }
    public List<FileSystemEntry> Children { get; } = new();
    public FileSystemEntry(string fullName, long size = 0)
    {
        FullName = fullName;
        Size = size;
    }
}