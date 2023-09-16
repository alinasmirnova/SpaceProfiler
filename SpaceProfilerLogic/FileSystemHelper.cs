using System.IO;
using System.Security;

namespace SpaceProfilerLogic;

public static class FileSystemHelper
{
    public static bool IsDirectoryAccessible(string path)
    {
        try
        {
            if (IsSymbolic(path))
                return false;
            
            if (Directory.Exists(path))
                Directory.EnumerateFileSystemEntries(path);
            
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static (bool, long) GetFileActualSize(string path)
    {
        try
        {
            if (IsSymbolic(path))
                return (false, 0);
            
            if (File.Exists(path))
                return (true, new FileInfo(path).Length);
            
            return (true, 0);
        }
        catch (Exception e)
        {
            return (false, 0);
        }
    }

    public static bool IsSymbolic(string path)
    {
        var pathInfo = new FileInfo(path);
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}