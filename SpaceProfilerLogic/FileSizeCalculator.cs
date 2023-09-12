using System.IO;

namespace SpaceProfilerLogic;

public static class FileSizeCalculator
{
    public static long GetFileSize(string fileName)
    {
        if (!FileSystemAccessHelper.IsAccessible(fileName))
            return 0;
        
        return new FileInfo(fileName).Length;
    }
}