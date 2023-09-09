using SpaceProfilerLogic.DirectoryWatcher;

namespace SpaceProfilerLogic.Tree;

public class FileSystemEntryTree
{
    public DirectoryEntry Root { get; }

    private readonly Dictionary<string, FileSystemEntry> nodes = new();

    public FileSystemEntryTree(string fullRootName)
    {
        if (!Directory.Exists(fullRootName))
            throw new ArgumentException(nameof(fullRootName));
        
        Root = new DirectoryEntry(fullRootName);
        nodes.Add(fullRootName, Root);
    }

    public FileSystemEntry?[] Apply(Change change)
    {
        switch (change.Type)
        {
            case ChangeType.Create:
                return Create(change.FullName);
            case ChangeType.Update:
                return Update(change.FullName);
            case ChangeType.Delete:
                return Delete(change.FullName);
            default:
                throw new ArgumentOutOfRangeException(nameof(change.Type));
        }
    }

    private FileSystemEntry?[] Create(string fullPath)
    {
        if (nodes.ContainsKey(fullPath) || !fullPath.StartsWith(Root.FullName))
            return Array.Empty<FileSystemEntry>();

        if (Directory.Exists(fullPath))
            return new[] { CreateDirectory(fullPath) };

        if (File.Exists(fullPath))
            return CreateFile(fullPath);

        return Array.Empty<FileSystemEntry>();
    }

    private DirectoryEntry? CreateDirectory(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath);
        var directory = new DirectoryEntry(fullPath, parent);
        if (parent.AddSubdirectory(directory))
        {
            nodes.Add(fullPath, directory);
            return directory;
        }

        return null;
    }

    private FileSystemEntry?[] CreateFile(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath);
        var file = new FileEntry(fullPath);
        if (!parent.AddFile(file))
            return new FileSystemEntry?[] { null }; 
        
        nodes.Add(fullPath, file);
        file.SetSize(FileSizeCalculator.GetFileSize(fullPath));
        
        var updated = new List<FileSystemEntry> { file };
        var current = file.Parent;
        while (current != null)
        {
            updated.Add(current);
            current = current.Parent;
        }
        
        return updated.ToArray();
    }

    private FileSystemEntry[] Update(string fullPath)
    {
        throw new NotImplementedException();
    }

    private FileSystemEntry[] Delete(string fullPath)
    {
        throw new NotImplementedException();
    }

    private DirectoryEntry FindOrCreateParent(string fullPath)
    {
        var missingParents = GetMissingParents(fullPath);
        CreateDirectories(missingParents);

        var parentName = Path.GetDirectoryName(fullPath);
        if (parentName == null || !nodes.ContainsKey(parentName))
            throw new KeyNotFoundException($"Failed to find parent node for path {fullPath}");
        
        return (DirectoryEntry)nodes[parentName];
    }

    private string[] GetMissingParents(string fullName)
    {
        var result = new List<string>();
        var current = Path.GetDirectoryName(fullName);
        while (current != null && !nodes.ContainsKey(current))
        {
            result.Add(current);
            current = Path.GetDirectoryName(fullName);
        }

        return result.ToArray();
    }

    private void CreateDirectories(string[] fullNames)
    {
        for (var index = fullNames.Length - 1; index >= 0; index--)
        {
            var fullName = fullNames[index];
            if (CreateDirectory(fullName) == null)
                return;
        }
    }
}