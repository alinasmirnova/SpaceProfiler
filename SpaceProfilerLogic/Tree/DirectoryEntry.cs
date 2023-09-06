namespace SpaceProfilerLogic.Tree;

public class DirectoryEntry : FileSystemEntry
{
    public List<DirectoryEntry> Subdirectories { get; } = new();
    public List<FileEntry> Files { get; } = new();
    
    public DirectoryEntry(string fullName, string? name, long size = 0) : base(fullName, name, size)
    {
    }
}