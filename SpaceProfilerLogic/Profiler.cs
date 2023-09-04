namespace SpaceProfilerLogic;

public class Profiler
{
    public FileInfo[] GetOrderedFiles(string? directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new ArgumentException("Directory does not exist");
        
        var result = new List<FileInfo>();

        var queue = new Queue<string>();
        queue.Enqueue(directoryPath);

        while (queue.TryDequeue(out var curDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(curDirectory))
            {
                result.Add(new FileInfo(file));
            }

            foreach (var directory in Directory.EnumerateDirectories(curDirectory))
            {
                queue.Enqueue(directory);
            }
        }
        
        return result.OrderByDescending(f => f.Length).ToArray();
    }
}