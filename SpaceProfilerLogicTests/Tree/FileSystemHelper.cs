namespace SpaceProfilerLogicTests.Tree;

public class FileSystemHelper
{
    public readonly string Root;
    public FileSystemHelper(string rootFolderName)
    {
        var currentDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty;
        Root = Path.GetFullPath(rootFolderName, currentDir);
        if (!Path.IsPathFullyQualified(Root))
            throw new ArgumentException("Incorrect root folder name", nameof(rootFolderName));
        
        CreateDirectoryInternal(Root);
    }

    public void CreateDirectory(string path)
    {
        CreateDirectoryInternal(Path.GetFullPath(path, Root));
    }

    public void CreateFile(string path, long size)
    {
        using var stream = File.Create(Path.GetFullPath(path, Root));
        stream.Write(new byte[size]);
        stream.Flush();
    }

    public void CreateFiles(long size, params string[] paths)
    {
        foreach (var path in paths)
        {
            CreateFile(path, size);
        }
    }

    public void Clear()
    {
        Directory.Delete(Root, true);
    }

    private void CreateDirectoryInternal(string fullPath)
    {
        if (Directory.Exists(fullPath))
            throw new ArgumentException($"Directory already exists: {fullPath}");
        Directory.CreateDirectory(fullPath);
    }
}