﻿using System.Collections.Concurrent;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public class SelfSustainableTree : IDisposable
{
    private readonly string rootFullPath;
    private readonly ConcurrentDictionary<FileSystemEntry, byte> changedNodes = new();
    private readonly ConcurrentQueue<string> pathsToRenew = new();
    
    private bool active;

    private readonly List<Thread> workers = new();
    private readonly DirectoryWatcher.DirectoryWatcher directoryWatcher;

    public DirectoryEntry Root => Tree.Root;
    public FileSystemEntryTree Tree { get; }
    public bool Loaded { get; private set; }

    public SelfSustainableTree(string rootFullPath)
    {
        if (!Directory.Exists(rootFullPath))
            throw new ArgumentException(nameof(rootFullPath));
        
        this.rootFullPath = rootFullPath;
        Tree = new FileSystemEntryTree(rootFullPath);
        directoryWatcher = new DirectoryWatcher.DirectoryWatcher();
        AddBackgroundWorker(LoadFromDisk);
        AddBackgroundWorker(WatchChanges);
        AddBackgroundWorker(ApplyChanges);
        AddBackgroundWorker(ApplyChanges);

        active = true;
        directoryWatcher.Start(this.rootFullPath);
        foreach (var worker in workers)
        {
            worker.Start();
        }
    }
    
    public HashSet<FileSystemEntry> GetChangedNodes()
    {
        var result = new HashSet<FileSystemEntry>();
        foreach (var changedNode in changedNodes)
        {
            if (changedNodes.TryRemove(changedNode.Key, out _))
                result.Add(changedNode.Key);
        }

        return result;
    }

    private void WatchChanges()
    {
        while (active)
        {
            foreach (var change in directoryWatcher.FlushChanges())
            {
                pathsToRenew.Enqueue(change);
            }
            Thread.Sleep(100);
        }
    }

    private void ApplyChanges()
    {
        while (active)
        {
            while (pathsToRenew.TryDequeue(out var path))
            {
                var changed = Tree.SynchronizeWithFileSystem(path).Where(e => e != null).Cast<FileSystemEntry>();
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
        Loaded = false;
        
        var queue = new Queue<string>();
        queue.Enqueue(rootFullPath);
        while (queue.TryDequeue(out var path) && active)
        {
            pathsToRenew.Enqueue(path);
            
            if (!FileSystemAccessHelper.IsAccessible(path))
                continue;

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                queue.Enqueue(directory);
            }
        }

        Loaded = true;
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

    public void Dispose()
    {
        StopSynchronization();
    }

    public void StopSynchronization()
    {
        active = false;
        directoryWatcher.Stop();
    }
}