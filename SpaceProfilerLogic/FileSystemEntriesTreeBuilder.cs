using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public static class FileSystemEntriesTreeBuilder
{
    public static TreeWatcher? Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            return null;
        
        var root = new DirectoryEntry(rootDirectory, Path.GetFileName(rootDirectory));
        var tree = new FileSystemEntryTree(root);
        var treeWatcher = new TreeWatcher(tree);
        treeWatcher.Start();
        return treeWatcher;
    }
}