using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

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
        var queue = new Queue<DirectoryEntry>();
        queue.Enqueue(tree.Root);
        while (queue.TryPeek(out var current))
        {
            foreach (var file in Directory.EnumerateFiles(current.FullName))
            {
                var child = new FileEntry(file, Path.GetFileName(file), current);
                if (current.AddFile(child))
                    child.SetSize(FileSizeCalculator.GetFileSize(file));
            }
            
            foreach (var directory in Directory.EnumerateDirectories(current.FullName))
            {
                var child = new DirectoryEntry(directory, Path.GetFileName(directory), current);
                if (current.AddSubdirectory(child))
                    queue.Enqueue(child);
            }
            
            queue.Dequeue();
        }
    }
}