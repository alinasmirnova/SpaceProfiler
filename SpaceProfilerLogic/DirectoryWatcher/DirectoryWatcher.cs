using System.Collections.Concurrent;

namespace SpaceProfilerLogic.DirectoryWatcher;

public class DirectoryWatcher
{
    private FileSystemWatcher? watcher;

    private readonly Queue<FileSystemEventArgs> changesQueue = new();

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

    public List<Change> FlushChanges()
    {
        var count = changesQueue.Count;
        var merger = new ChangesMerger();
        
        for (var i = 0; i < count; i++)
        {
            if (!changesQueue.TryDequeue(out var change)) continue;

            var oldPath = (change as RenamedEventArgs)?.OldFullPath;
            if (oldPath != null)
            {
                merger.Push(new Change(oldPath, ChangeType.Delete));
                merger.Push(new Change(change.FullPath, ChangeType.Create));
            }
            else
            {
                merger.Push(new Change(change));
            }
        }
        return merger.Merged;
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