namespace SpaceProfilerLogic.Tree;

public static class FileSystemEntriesTreeBuilder
{
    public static FileSystemEntryTree? Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            return null;
        
        var root = new FileSystemEntry(rootDirectory, Path.GetFileName(rootDirectory));
        var tree = new FileSystemEntryTree(root);
        Fill(tree);
        return tree;
    }

    private static void Fill(FileSystemEntryTree tree)
    {
        var stack = new Stack<FileSystemEntry>();
        stack.Push(tree.Root);
        while (stack.TryPeek(out var current))
        {
            if (!current.Children.Any())
            {
                var subDirectories = AddDirectories(current).ToArray();
                foreach (var subDirectory in subDirectories)
                {
                    stack.Push(subDirectory);
                }
                
                if (subDirectories.Any())
                    continue;
            }

            current.Size = SumChildren(current);
            AddFiles(current);
            stack.Pop();
        }
    }

    private static IEnumerable<FileSystemEntry> AddDirectories(FileSystemEntry entry)
    {
        foreach (var directory in Directory.EnumerateDirectories(entry.FullName))
        {
            var child = new FileSystemEntry(directory, Path.GetFileName(directory));
            entry.Children.Add(child);
            yield return child;
        }
    }

    private static long SumChildren(FileSystemEntry entry)
    {
        return entry.Children.Sum(c => c.Size);
    }

    private static void AddFiles(FileSystemEntry entry)
    {
        foreach (var file in Directory.EnumerateFiles(entry.FullName))
        {
            var child = new FileSystemEntry(file, Path.GetFileName(file), FileSizeCalculator.GetFileSize(file));
            entry.Children.Add(child);
            entry.Size += child.Size;
        }
    }
}