using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SpaceProfilerLogicTests.TestHelpers;

[SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
public class FileSystemHelper
{
    public readonly string Root;
    public FileSystemHelper(string rootFolderName)
    {
        var currentDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty;
        Root = Path.GetFullPath(rootFolderName, currentDir);
        if (!Path.IsPathFullyQualified(Root))
            throw new ArgumentException("Incorrect root folder name", nameof(rootFolderName));
        
        Clear();
        CreateDirectoryInternal(Root);
    }

    public void Clear()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, true);
    }

    public void Delete(string path)
    {
        if (path == Root)
        {
            Clear();
            return;
        }
            
        var fullPath = Path.GetFullPath(path, Root);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        else if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, true);
    }

    public void CreateFiles(long size, params string[] paths)
    {
        foreach (var path in paths)
        {
            CreateFile(path, size);
        }
    }

    public void CreateFile(string path, long size)
    {
        using var stream = File.Create(Path.GetFullPath(path, Root));
        stream.Write(new byte[size]);
        stream.Flush();
    }

    public void CreateDirectory(string path)
    {
        CreateDirectoryInternal(Path.GetFullPath(path, Root));
    }
    
    public void CreateDirectoryWithFiles(string path, long fileSize, params string[] fileNames)
    {
        CreateDirectoryInternal(Path.GetFullPath(path, Root));
        CreateFiles(fileSize, fileNames.Select(n => $"{path}\\{n}").ToArray());
    }

    public void ChangeAccessRights(string path)
    {
        var fullPath = Path.GetFullPath(path, Root);
        if (Directory.Exists(fullPath))
        {
            var directoryInfo = new DirectoryInfo(fullPath)!;
            
            var directorySecurity = directoryInfo.GetAccessControl();
            var fileSystemRule = CreateFileSystemAccessRule();
            directorySecurity.AddAccessRule(fileSystemRule);
            directoryInfo.SetAccessControl(directorySecurity);
        }
    }

    private static FileSystemAccessRule CreateFileSystemAccessRule()
    {
        var currentUserIdentity = WindowsIdentity.GetCurrent();
        var fileSystemRule = new FileSystemAccessRule(currentUserIdentity.Name,
            FileSystemRights.Read,
            InheritanceFlags.ObjectInherit |
            InheritanceFlags.ContainerInherit,
            PropagationFlags.None,
            AccessControlType.Allow);
        return fileSystemRule;
    }

    private void CreateDirectoryInternal(string fullPath)
    {
        if (Directory.Exists(fullPath))
            throw new ArgumentException($"Directory already exists: {fullPath}");
        Directory.CreateDirectory(fullPath);
    }
}