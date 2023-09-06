namespace SpaceProfilerLogic.Tree;

public class DirectoryEntry : FileSystemEntry
{
    private readonly List<FileEntry> files = new();

    public List<FileEntry> Files
    {
        get => files;
        init
        {
            files = value;
            foreach (var file in files)
            {
                file.Parent = this;
            }
        }
    }
    
    private readonly List<DirectoryEntry> subdirectories = new();
    public List<DirectoryEntry> Subdirectories
    {
        get => subdirectories;
        init
        {
            subdirectories = value;
            foreach (var subdirectory in subdirectories)
            {
                subdirectory.Parent = this;
            }
        }
    }

    public DirectoryEntry(string fullName, string? name, long size = 0, FileSystemEntry? parent = null) : base(fullName, name, size, parent)
    {
    }
}