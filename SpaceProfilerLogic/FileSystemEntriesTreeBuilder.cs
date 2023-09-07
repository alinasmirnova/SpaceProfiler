using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public static class FileSystemEntriesTreeBuilder
{
    public static TreeWatcher Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new ArgumentException("Directory not exist");
        
        var root = new DirectoryEntry(rootDirectory, Path.GetFileName(rootDirectory));
        var tree = new FileSystemEntryTree(root);
        var treeWatcher = new TreeWatcher(tree);
        return treeWatcher;
    }
}