namespace SpaceProfilerLogic.Tree;

public class FileEntry : FileSystemEntry
{
    public FileEntry(string fullName, FileSystemEntry? parent = null) : base(fullName, parent)
    {
    }
    
    public FileEntry(string fullName, long size) : base(fullName)
    {
        Size = size;
    }
}