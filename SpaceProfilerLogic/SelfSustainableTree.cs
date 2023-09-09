using System.Collections.Concurrent;
using SpaceProfilerLogic.DirectoryWatcher;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public class SelfSustainableTree
{
    private readonly FileSystemEntryTree? tree;
    private readonly string rootFullPath;
    private readonly ConcurrentDictionary<FileSystemEntry, byte> changedNodes = new();
    private readonly ConcurrentQueue<Change> changesToApply = new();
    
    private bool active = false;

    private readonly List<Thread> workers = new();
    private readonly DirectoryWatcher.DirectoryWatcher directoryWatcher;

    public DirectoryEntry? Root => tree?.Root;

    public SelfSustainableTree(string rootFullPath)
    {
        if (!Directory.Exists(rootFullPath))
            throw new ArgumentException(nameof(rootFullPath));
        
        this.rootFullPath = rootFullPath;
        tree = new FileSystemEntryTree(rootFullPath);
        directoryWatcher = new DirectoryWatcher.DirectoryWatcher();
        AddBackgroundWorker(LoadFromDisk);
        AddBackgroundWorker(WatchChanges);
        AddBackgroundWorker(ApplyChanges);
        AddBackgroundWorker(ApplyChanges);
    }
    
    public void StartSynchronization()
    {
        active = true;
        directoryWatcher.Start(rootFullPath);
        foreach (var worker in workers)
        {
            worker.Start();
        }
    }

    public void StopSynchronization()
    {
        active = false;
        directoryWatcher.Stop();
    }

    public FileSystemEntry[] GetChangedNodes()
    {
        var result = new List<FileSystemEntry>();
        foreach (var changedNode in changedNodes)
        {
            if (changedNodes.TryRemove(changedNode.Key, out var _))
                result.Add(changedNode.Key);
        }

        return result.ToArray();
    }

    private void WatchChanges()
    {
        while (active)
        {
            foreach (var change in directoryWatcher.FlushChanges())
            {
                changesToApply.Enqueue(change);
            }
            Thread.Sleep(100);
        }
    }

    private void ApplyChanges()
    {
        while (active)
        {
            while (changesToApply.TryDequeue(out var change))
            {
                var changed = tree.Apply(change).Where(e => e != null).Cast<FileSystemEntry>();
                foreach (var entry in changed)
                {
                    changedNodes.AddOrUpdate(entry, 0, (_, _) => 0);
                }
            }
            Thread.Sleep(0);
        }
    }

    private void LoadFromDisk()
    {
        var queue = new Queue<string>();
        queue.Enqueue(rootFullPath);
        while (queue.TryDequeue(out var path) && active)
        {
            changesToApply.Enqueue(new Change(path, ChangeType.Create));

            foreach (var file in Directory.EnumerateFiles(path))
            {
                changesToApply.Enqueue(new Change(file, ChangeType.Create));
            }

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                queue.Enqueue(directory);
            }
        }
    }

    private void AddBackgroundWorker(Action action)
    {
        var thread = new Thread(() => action())
        {
            IsBackground = true
        };
        workers.Add(thread);
        
        if (active)
            thread.Start();
    }
}