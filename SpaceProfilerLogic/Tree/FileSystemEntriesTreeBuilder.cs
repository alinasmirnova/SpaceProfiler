namespace SpaceProfilerLogic.Tree;

public static class FileSystemEntriesTreeBuilder
{
    public static FileSystemEntryTree? Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            return null;
        
        var root = new DirectoryEntry(rootDirectory, Path.GetFileName(rootDirectory));
        var tree = new FileSystemEntryTree(root);
        Fill(tree);
        return tree;
    }

    private static void Fill(FileSystemEntryTree tree)
    {
        var stack = new Queue<DirectoryEntry>();
        stack.Enqueue(tree.Root);
        while (stack.TryPeek(out var current))
        {
            if (!current.Subdirectories.Any())
            {
                var subDirectories = AddDirectories(current).ToArray();
                foreach (var subDirectory in subDirectories)
                {
                    stack.Enqueue(subDirectory);
                }
            }

            AddFiles(current);
            stack.Dequeue();
        }
    }

    private static IEnumerable<DirectoryEntry> AddDirectories(DirectoryEntry entry)
    {
        foreach (var directory in Directory.EnumerateDirectories(entry.FullName))
        {
            var child = new DirectoryEntry(directory, Path.GetFileName(directory), entry);
            entry.Subdirectories.Add(child);
            yield return child;
        }
    }

    private static void AddFiles(DirectoryEntry entry)
    {
        foreach (var file in Directory.EnumerateFiles(entry.FullName))
        {
            var child = new FileEntry(file, Path.GetFileName(file), entry);
            entry.Files.Add(child);
            child.Size = FileSizeCalculator.GetFileSize(file);
        }
    }
}