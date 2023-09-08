using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public static class SelfSustainableTreeBuilder
{
    public static SelfSustainableTree Build(string? rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new ArgumentException("Directory not exist");
        
        var root = new DirectoryEntry(rootDirectory);
        var tree = new FileSystemEntryTree(root);
        var treeWatcher = new SelfSustainableTree(tree);
        return treeWatcher;
    }
}