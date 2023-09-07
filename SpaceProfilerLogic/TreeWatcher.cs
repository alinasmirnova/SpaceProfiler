using System.Collections.Concurrent;
using SpaceProfilerLogic.Tree;

namespace SpaceProfilerLogic;

public class TreeWatcher
{
    private readonly ConcurrentQueue<DirectoryEntry> changesQueue;
    private readonly Thread[] workers;
    public FileSystemEntryTree Tree { get; }

    private bool stopped = true;

    public TreeWatcher(FileSystemEntryTree tree)
    {
        Tree = tree;
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

    private void ProcessChanges()
    {
        while (!stopped)
        {
            if (changesQueue.TryDequeue(out var directory))
            {
                Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} processing directory {directory.Name}");
                
                directory.Update(out var childrenNeedUpdate);
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