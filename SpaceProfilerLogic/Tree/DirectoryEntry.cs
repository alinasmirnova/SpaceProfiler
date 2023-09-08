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

    public DirectoryEntry(string fullName, FileSystemEntry? parent = null) : base(fullName, parent)
    {
    }
    
    public DirectoryEntry(string fullName, long size) : base(fullName)
    {
        this.size = size;
    }

    public void Update(out bool childrenNeedUpdate)
    {
        foreach (var file in Directory.EnumerateFiles(FullName))
        {
            var child = new FileEntry(file, this);
            if (AddFile(child))
                child.SetSize(FileSizeCalculator.GetFileSize(file));
        }
        
        childrenNeedUpdate = false;
        foreach (var directory in Directory.EnumerateDirectories(FullName))
        {
            var child = new DirectoryEntry(directory, this);
            if (AddSubdirectory(child))
                childrenNeedUpdate = true;
        }
    }
}