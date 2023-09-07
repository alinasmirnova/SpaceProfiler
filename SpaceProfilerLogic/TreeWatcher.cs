using System.Collections.Concurrent;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public class TreeWatcher
{
    private readonly ConcurrentQueue<DirectoryEntry> changesQueue;
    private readonly Thread[] workers;
    private readonly ConcurrentDictionary<DirectoryEntry, byte> updated;
    public FileSystemEntryTree Tree { get; }

    private bool stopped = true;

    public TreeWatcher(FileSystemEntryTree tree)
    {
        Tree = tree;
        updated = new ConcurrentDictionary<DirectoryEntry, byte>();
        changesQueue = new ConcurrentQueue<DirectoryEntry>();
        changesQueue.Enqueue(tree.Root);
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
                Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} processing directory {directory.Name}");
                
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

    public void Start()
    {
        stopped = false;
        foreach (var worker in workers)
        {
            worker.Start();
        }
    }

    public void Stop()
    {
        stopped = true;
    }
}