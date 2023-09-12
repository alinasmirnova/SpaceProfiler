namespace SpaceProfiler.Helpers;

public static class FileSizeHelper
{
    private static readonly string[] Options = { "B", "KB", "MB", "GB", "TB" };
    public static string ToHumanReadableString(long fileSize)
    {
        var i = 0;
        var current = (double) fileSize;
        while (current > 1024 && i < Options.Length - 1)
        {
            current /= 1024;
            i++;
        }

        return $"{current:N1} {Options[i]}";
    }
}