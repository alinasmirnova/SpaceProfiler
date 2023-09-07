using System.Collections.Concurrent;

namespace SpaceProfilerLogic.Tree;

public class DirectoryEntry : FileSystemEntry
{
    private readonly ConcurrentDictionary<string, FileEntry> files = new();

    public FileEntry[] Files
    {
        get => files.Values.ToArray();
        init
        {
            foreach (var file in value)
            {
                if (files.TryAdd(file.Name, file))
                    file.Parent = this;
            }
        }
    }

    public bool AddFile(FileEntry file) => files.TryAdd(file.Name, file);
    
    private readonly ConcurrentDictionary<string, DirectoryEntry> subdirectories = new();
    public DirectoryEntry[] Subdirectories
    {
        get => subdirectories.Values.ToArray();
        init
        {
            foreach (var subdirectory in value)
            {
                if (subdirectories.TryAdd(subdirectory.Name, subdirectory))
                    subdirectory.Parent = this;
            }
        }
    }

    public bool AddSubdirectory(DirectoryEntry directoryEntry) =>
        subdirectories.TryAdd(directoryEntry.Name, directoryEntry);

    public DirectoryEntry(string fullName, string? name, FileSystemEntry? parent = null) : base(fullName, name, parent)
    {
    }
    
    public DirectoryEntry(string fullName, string? name, long size) : base(fullName, name)
    {
        this.size = size;
    }
}