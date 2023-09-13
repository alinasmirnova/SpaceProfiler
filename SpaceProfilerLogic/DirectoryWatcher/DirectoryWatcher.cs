using System.Collections.Concurrent;

namespace SpaceProfilerLogic.DirectoryWatcher;

public class DirectoryWatcher
{
    private FileSystemWatcher? watcher;

    private readonly ConcurrentQueue<FileSystemEventArgs> changesQueue = new();

    public void Start(string root)
    {
        if (!FileSystemAccessHelper.IsAccessible(root))
            return;
        
        watcher = new FileSystemWatcher();
        watcher.Path = Path.GetFullPath(root);
        watcher.NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Security
                               | NotifyFilters.Size;

        
        watcher.Filter = "*.*";
        watcher.IncludeSubdirectories = true;
        
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        
        watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        if (watcher is not { EnableRaisingEvents: true }) return;
        watcher.EnableRaisingEvents = false;
        
        watcher.Changed -= OnChanged;
        watcher.Created -= OnCreated;
        watcher.Deleted -= OnDeleted;
        watcher.Renamed -= OnRenamed;
    }

    public HashSet<string> FlushChanges()
    {
        var count = changesQueue.Count;
        var result = new HashSet<string>();

        for (var i = 0; i < count; i++)
        {
            if (!changesQueue.TryDequeue(out var change)) continue;

            var oldPath = (change as RenamedEventArgs)?.OldFullPath;
            if (oldPath != null)
            {
                result.Add(oldPath);
            }
            
            if (change.ChangeType == WatcherChangeTypes.Created)
                PushWithSubdirectoriesRecursively(result, change.FullPath);
            else
                result.Add(change.FullPath);
        }

        return result;
    }

    private void PushWithSubdirectoriesRecursively(HashSet<string> result, string path)
    {
        result.Add(path);
        
        var queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.TryDequeue(out var current))
        {
            if (!Directory.Exists(current))
                continue;
            
            result.Add(current);
            
            if (!FileSystemAccessHelper.IsAccessible(current))
                continue;

            foreach (var entry in Directory.EnumerateFileSystemEntries(current))
            {
                queue.Enqueue(entry);
            }
        }
    }
    
    public void OnChanged(object sender, FileSystemEventArgs e)
    {
        changesQueue.Enqueue(e);
    }

    public void OnCreated(object sender, FileSystemEventArgs e)
    {
        changesQueue.Enqueue(e);
    }

    public void OnDeleted(object sender, FileSystemEventArgs e)
    {
        changesQueue.Enqueue(e);
    }

    public void OnRenamed(object sender, RenamedEventArgs e)
    {
        changesQueue.Enqueue(e);
    }
}