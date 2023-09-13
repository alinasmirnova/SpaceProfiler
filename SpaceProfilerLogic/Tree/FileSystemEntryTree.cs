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
        
        Root = CreateDirectory(fullRootName, null);
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

        var result = createdParents.Concat(createdParents.Select(p => p.Parent)
                .Concat(createdParents.SelectMany(p => p.Files)).Where(p => p != null))
            .Cast<FileSystemEntry>().ToList();

        if (directory != null)
        {
            result.Add(directory);
            foreach (var fileEntry in directory.Files)
            {
                result.Add(fileEntry);
            }
        }

        if (directory?.Parent != null)
        {
            result.Add(directory.Parent);
            if (directory.GetSize > 0 || createdParents.Any())
            {
                result = result.Concat(GetParents(directory.Parent)).ToList();
            }
        }

        return result.Distinct().ToArray();
    }

    private DirectoryEntry? CreateDirectory(string fullPath, DirectoryEntry? parent)
    {
        DirectoryEntry result;

        var files = new List<FileEntry>();
        if (FileSystemAccessHelper.IsAccessible(fullPath))
        {
            long sum = 0;
            foreach (var file in Directory.EnumerateFiles(fullPath))
            {
                if (!File.Exists(file))
                    continue;
                
                var fileSize = FileSizeCalculator.GetFileSize(file);
                files.Add(new FileEntry(file, fileSize));
                sum += fileSize;
            }

            result = new DirectoryEntry(fullPath, sum, parent)
            {
                Files = files.ToArray()
            };
        }
        else
        {
            result = new DirectoryEntry(fullPath, parent);
        }
        
        if (parent == null || parent.AddSubdirectory(result))
        {
            nodes.TryAdd(result.FullName, result);
            foreach (var file in result.Files)
            {
                nodes.TryAdd(file.FullName, file);
            }
            return result;
        }

        return null;
    }

    private FileSystemEntry[] CreateFile(string fullPath)
    {
        var parent = FindOrCreateParent(fullPath, out var createdParents);
        if (parent == null)
            throw new KeyNotFoundException($"Failed to find parent for {fullPath}");

        if (createdParents.Any())
            return createdParents.Concat(GetParents(parent))
                .Concat(createdParents.SelectMany(p => p.Files)).Distinct().ToArray();
       
        var file = new FileEntry(fullPath, FileSizeCalculator.GetFileSize(fullPath));
        if (!parent.AddFile(file))
            return Array.Empty<FileSystemEntry>();

        nodes.TryAdd(file.FullName, file);

        var result = file.GetSize > 0 ? GetParents(file) : new List<FileSystemEntry>();
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
        {
            var result = GetParents(file);
            result.Add(file);
            return result.ToArray();
        }

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
                Root = null;
                nodes.Clear();
                return Array.Empty<FileSystemEntry>();
            }

            var parent = (DirectoryEntry)entry.Parent!;
            if (entry is DirectoryEntry && !parent.RemoveSubdirectory(entry))
                return Array.Empty<FileSystemEntry>();

            if (entry is FileEntry && !parent.RemoveFile(entry))
                return Array.Empty<FileSystemEntry>();

            RemoveFromNodesWithSubElements(fullPath);

            var result = new List<FileSystemEntry>() { parent };
            if (entry.GetSize > 0)
            {
                result = result.Concat(GetParents(parent)).ToList();
            }
            
            return result.ToArray();
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

    private static List<FileSystemEntry> GetParents(FileSystemEntry entry)
    {
        var updated = new List<FileSystemEntry>();
        var current = entry.Parent;
        while (current != null)
        {
            updated.Add(current);
            current = current.Parent;
        }

        return updated;
    }

    private string GetParentFullName(string path)
    {
        var parts = path.Split('\\').ToArray();
        return string.Join('\\', parts.Take(parts.Length - 1));
    }
}