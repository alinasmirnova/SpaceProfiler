namespace SpaceProfilerLogic;

public static class FileSizeCalculator
{
    public static long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }
}