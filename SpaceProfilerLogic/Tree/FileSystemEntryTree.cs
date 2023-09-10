using System.Collections.Concurrent;
using SpaceProfilerLogic.DirectoryWatcher;

namespace SpaceProfilerLogic.Tree;

public class FileSystemEntryTree
{
    public DirectoryEntry? Root { get; set; }

    private readonly ConcurrentDictionary<string, FileSystemEntry> nodes = new();

    public FileSystemEntryTree(string fullRootName)
    {
        if (!Directory.Exists(fullRootName))
            throw new ArgumentException(nameof(fullRootName));
        
        Root = new DirectoryEntry(fullRootName);
        nodes.TryAdd(fullRootName, Root);
    }

    public FileSystemEntry?[] Apply(Change change)
    {
        return change.Type switch
        {
            ChangeType.Create => Create(change.FullName),
            ChangeType.Update => Update(change.FullName),
            ChangeType.Delete => Delete(change.FullName),
            _ => throw new ArgumentOutOfRangeException(nameof(change.Type))
        };
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
        if (parent.AddEmptySubdirectory(directory))
        {
            nodes.TryAdd(fullPath, directory);
            return directory;
        }

        return null;
    }

    private FileSystemEntry?[] CreateFile(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath);
        var file = new FileEntry(fullPath, FileSizeCalculator.GetFileSize(fullPath));
        if (!parent.AddFile(file))
            return new FileSystemEntry?[] { null }; 
        
        nodes.TryAdd(fullPath, file);

        return GetCurrentAndParents(file).ToArray();
    }

    private FileSystemEntry?[] Update(string fullPath)
    {
        if (Directory.Exists(fullPath))
            return Array.Empty<FileSystemEntry>();

        if (File.Exists(fullPath))
            return ChangeFile(fullPath);
        
        return Array.Empty<FileSystemEntry>();
    }

    private FileSystemEntry?[] ChangeFile(string fullPath)
    {
        if (!nodes.ContainsKey(fullPath))
            return CreateFile(fullPath);

        var file = nodes[fullPath];
        var size = FileSizeCalculator.GetFileSize(fullPath);
        var diff = size - file.GetSize;
        if (file.AddSize(diff))
            return GetCurrentAndParents(file);

        return Array.Empty<FileSystemEntry>();
    }

    private readonly object lockForDelete = new();
    private FileSystemEntry?[] Delete(string fullPath)
    {
        lock (lockForDelete)
        {
            if (!nodes.ContainsKey(fullPath))
                return Array.Empty<FileSystemEntry>();

            var entry = nodes[fullPath];
            if (entry == Root)
            {
                var result = Root;
                Root = null;
                nodes.Clear();
                return new FileSystemEntry[] { result };
            }

            var parent = (DirectoryEntry)entry.Parent!;
            if (entry is DirectoryEntry && !parent.RemoveSubdirectory(entry))
                return Array.Empty<FileSystemEntry>();

            if (entry is FileEntry && !parent.RemoveFile(entry))
                return Array.Empty<FileSystemEntry>();

            RemoveFromNodesWithSubElements(fullPath);
            return GetCurrentAndParents(parent);
        }
    }

    private void RemoveFromNodesWithSubElements(string fullName)
    {
        foreach (var node in nodes)
        {
            if (node.Key.StartsWith(fullName) && !Directory.Exists(node.Key) && !File.Exists(node.Key))
                nodes.TryRemove(node);
        }
    }

    private static FileSystemEntry?[] GetCurrentAndParents(FileSystemEntry? entry)
    {
        var updated = new List<FileSystemEntry?> { entry };
        var current = entry?.Parent;
        while (current != null)
        {
            updated.Add(current);
            current = current.Parent;
        }

        return updated.ToArray();
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