using System.Collections.Concurrent;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public class SelfSustainableTree
{
    private readonly ConcurrentQueue<DirectoryEntry> changesQueue;
    private readonly Thread[] workers;
    private readonly ConcurrentDictionary<DirectoryEntry, byte> updated;
    
    private readonly FileSystemEntryTree tree;
    private bool stopped = true;

    public DirectoryEntry Root => tree.Root;

    public SelfSustainableTree(FileSystemEntryTree tree)
    {
        this.tree = tree;
        updated = new ConcurrentDictionary<DirectoryEntry, byte>();
        changesQueue = new ConcurrentQueue<DirectoryEntry>();
        workers = new[] { CreateWorkerThread(), CreateWorkerThread() };
    }

    private Thread CreateWorkerThread()
    {
        var workerThread = new Thread(ProcessChanges)
        {
            IsBackground = true
        };
        return workerThread;
    }

    public DirectoryEntry[] FlushUpdated()
    {
        var result = new List<DirectoryEntry>();
        foreach (var u in updated)
        {
            if (updated.TryRemove(u))
                result.Add(u.Key);
            
        }
        return result.ToArray();
    } 

    private void ProcessChanges()
    {
        while (!stopped)
        {
            if (changesQueue.TryDequeue(out var directory))
            {
                directory.Update(out var childrenNeedUpdate);
                updated.TryAdd(directory, 1);
                
                if (childrenNeedUpdate)
                    foreach (var subdirectory in directory.Subdirectories)
                    {
                        changesQueue.Enqueue(subdirectory);
                    }
            }
            else
            {
                Thread.Sleep(0);
            }
        }
    }

    public void StartSynchronization()
    {
        stopped = false;
        changesQueue.Enqueue(Root);
        foreach (var worker in workers)
        {
            worker.Start();
        }
    }

    public void StopSynchronization()
    {
        stopped = true;
    }
}