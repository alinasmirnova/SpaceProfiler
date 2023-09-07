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
            foreach (var file in Directory.EnumerateFiles(current.FullName))
            {
                var child = new FileEntry(file, Path.GetFileName(file), current);
                current.Files.Add(child);
                child.SetSize(FileSizeCalculator.GetFileSize(file));
            }
            
            foreach (var directory in Directory.EnumerateDirectories(current.FullName))
            {
                var child = new DirectoryEntry(directory, Path.GetFileName(directory), current);
                current.Subdirectories.Add(child);
                stack.Enqueue(child);
            }
            
            stack.Dequeue();
        }
    }
}