namespace SpaceProfilerLogic.Tree;

public class FileSystemEntryTree
{
    public DirectoryEntry Root { get; }

    public FileSystemEntryTree(DirectoryEntry root)
    {
        Root = root;
    }
}