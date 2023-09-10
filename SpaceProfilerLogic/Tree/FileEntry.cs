namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, long size) : base(fullName)
    {
        Size = size;
    }
}