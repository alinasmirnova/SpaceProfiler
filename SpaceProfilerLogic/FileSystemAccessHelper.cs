using System.IO;
using System.Security;

namespace SpaceProfilerLogic;

public static class FileSystemAccessHelper
{
    public static bool IsAccessible(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.EnumerateDirectories(path);
            return true;
        }
        catch (UnauthorizedAccessException e)
        {
            return false;
        }
    }
}