namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, long size, bool isAccessible) : base(fullName, isAccessible, null)
    {
        Size = size;
    }

    public FileEntry(string fullName, int size) : this(fullName, size, true)
    {
    }
}