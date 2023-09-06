namespace SpaceProfilerLogic.Tree;

public class FileSystemEntryTree
{
    public FileSystemEntry Root { get; }

    public FileSystemEntryTree(FileSystemEntry root)
    {
        Root = root;
    }
}