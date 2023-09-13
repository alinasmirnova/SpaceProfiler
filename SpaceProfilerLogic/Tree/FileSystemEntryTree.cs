using System.Collections.Concurrent;

namespace SpaceProfilerLogic.Tree;

public class FileSystemEntryTree
{
    public DirectoryEntry? Root { get; set; }

    private readonly ConcurrentDictionary<string, FileSystemEntry> nodes = new();

    public FileSystemEntryTree(string fullRootName)
    {
        if (!Directory.Exists(fullRootName))
            throw new ArgumentException(nameof(fullRootName));
        fullRootName = fullRootName.TrimEnd('\\');
        
        Root = new DirectoryEntry(fullRootName);
        nodes.TryAdd(Root.FullName, Root);
    }

    public FileSystemEntry?[] SynchronizeWithFileSystem(string path)
    {
        path = path.TrimEnd('\\');
        if (Root == null || !path.StartsWith(Root.FullName))
            return Array.Empty<FileSystemEntry>();
        
        if (Directory.Exists(path))
        {
            if (nodes.ContainsKey(path))
               return Update(path);

            return CreateDirectory(path);
        }

        if (File.Exists(path))
        {
            if (nodes.ContainsKey(path))
                return Update(path);

            return CreateFile(path);
        }

        if (nodes.ContainsKey(path))
            return Delete(path);

        return Array.Empty<FileSystemEntry>();
    }

    private FileSystemEntry[] CreateDirectory(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath, out var createdParents);
        if (parent == null)
            throw new KeyNotFoundException($"Failed to find parent for {fullPath}");

        var directory = CreateDirectory(fullPath, parent);

        var result = createdParents.Concat(createdParents.Select(p => p.Parent).Where(p => p != null))
            .Cast<FileSystemEntry>().ToList();
        
        if (directory != null)
            result.Add(directory);
        
        if (directory?.Parent != null)
            result.Add(directory.Parent);

        return result.Distinct().ToArray();
    }

    private DirectoryEntry? CreateDirectory(string fullPath, DirectoryEntry parent)
    {
        var directory = new DirectoryEntry(fullPath, parent);
        if (parent.AddEmptySubdirectory(directory))
        {
            nodes.TryAdd(directory.FullName, directory);
            return directory;
        }

        return null;
    }

    private FileSystemEntry[] CreateFile(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath, out var createdParents);
        if (parent == null)
            throw new KeyNotFoundException($"Failed to find parent for {fullPath}");
        
        var file = new FileEntry(fullPath, FileSizeCalculator.GetFileSize(fullPath));
        var result = createdParents.Concat(createdParents.Select(p => p.Parent).Where(p => p != null))
            .Cast<FileSystemEntry>().Distinct().ToList();
        
        if (!parent.AddFile(file))
        {
            return result.ToArray();
        } 
        
        nodes.TryAdd(file.FullName, file);

        if (file.GetSize > 0)
            return GetCurrentAndParents(file).ToArray();
        
        result.Add(parent);
        result.Add(file);
        return result.Distinct().ToArray();
    }

    private DirectoryEntry? FindOrCreateParent(string fullPath, out DirectoryEntry[] createdParents)
    {
        createdParents = Array.Empty<DirectoryEntry>();
        
        var missingParents = GetMissingParents(fullPath, out var closestParent);
        if (closestParent == null)
            return null;
        
        createdParents = CreateDirectories(missingParents, closestParent);

        var parentName = GetParentFullName(fullPath);
        if (!nodes.ContainsKey(parentName))
            throw new KeyNotFoundException($"Failed to find parent node for path {fullPath}");
        
        return (DirectoryEntry)nodes[parentName];
    }
    
    private string[] GetMissingParents(string fullName, out DirectoryEntry? closestParent)
    {
        var result = new List<string>();
        var current = GetParentFullName(fullName);
        while (current != string.Empty && !nodes.ContainsKey(current))
        {
            result.Add(current);
            current = GetParentFullName(current);
        }

        closestParent = (DirectoryEntry)nodes[current];
        return result.ToArray();
    }
    
    private DirectoryEntry[] CreateDirectories(string[] fullNames, DirectoryEntry parent)
    {
        var created = new List<DirectoryEntry>();
        var lastCreated = parent;
        for (var index = fullNames.Length - 1; index >= 0; index--)
        {
            var fullName = fullNames[index];
            var directory = CreateDirectory(fullName, lastCreated);
            if (directory == null)
                break;
            
            lastCreated = directory;
            created.Add(directory);
        }

        return created.ToArray();
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
            
            if (entry.GetSize > 0)
                return GetCurrentAndParents(parent);
            
            return new FileSystemEntry?[] { parent };
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

    private static FileSystemEntry[] GetCurrentAndParents(FileSystemEntry entry)
    {
        var updated = new List<FileSystemEntry> { entry };
        var current = entry?.Parent;
        while (current != null)
        {
            updated.Add(current);
            current = current.Parent;
        }

        return updated.ToArray();
    }

    private string GetParentFullName(string path)
    {
        var parts = path.Split('\\').ToArray();
        return string.Join('\\', parts.Take(parts.Length - 1));
    }
}