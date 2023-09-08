using System.Collections.Concurrent;

namespace SpaceProfilerLogic.DirectoryWatcher;

public class DirectoryWatcher
{
    private FileSystemWatcher? watcher;

    private readonly ConcurrentQueue<FileSystemEventArgs> changes = new();

    public void Start(string root)
    {
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
        if (watcher == null) return;
        watcher.EnableRaisingEvents = false;
        
        watcher.Changed -= OnChanged;
        watcher.Created -= OnCreated;
        watcher.Deleted -= OnDeleted;
        watcher.Renamed -= OnRenamed;
    }

    public List<FileSystemEventArgs> FlushChanges()
    {
        var result = new List<FileSystemEventArgs>();
        var count = changes.Count;
        for (var i = 0; i < count; i++)
        {
            if(changes.TryDequeue(out var change))
                result.Add(change);
        }
        return result;
    }

    public void OnChanged(object sender, FileSystemEventArgs e)
    {
        changes.Enqueue(e);
    }

    public void OnCreated(object sender, FileSystemEventArgs e)
    {
        changes.Enqueue(e);
    }

    public void OnDeleted(object sender, FileSystemEventArgs e)
    {
        changes.Enqueue(e);
    }

    public void OnRenamed(object sender, RenamedEventArgs e)
    {
        changes.Enqueue(e);
    }
}