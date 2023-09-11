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
        if (watcher is not { EnableRaisingEvents: true }) return;
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
            else if (change.ChangeType == WatcherChangeTypes.Created)
            {
                PushCreateRecursively(merger, change);
            }
            else
            {
                merger.Push(new Change(change));
            }
        }
        return merger.Merged;
    }

    private void PushCreateRecursively(ChangesMerger merger, FileSystemEventArgs change)
    {
        var queue = new Queue<string>();
        queue.Enqueue(change.FullPath);
        while (queue.TryDequeue(out var current))
        {
            merger.Push(new Change(current, ChangeType.Create));
            if (File.Exists(current))
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