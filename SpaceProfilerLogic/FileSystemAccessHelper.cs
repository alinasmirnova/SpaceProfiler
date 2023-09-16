using System.IO;
using System.Security;

namespace SpaceProfilerLogic;

public static class FileSystemAccessHelper
{
    public static bool IsDirectoryAccessible(string path)
    {
        try
        {
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
            if (File.Exists(path))
                return (true, new FileInfo(path).Length);
            
            return (true, 0);
        }
        catch (Exception e)
        {
            return (false, 0);
        }
    }
}