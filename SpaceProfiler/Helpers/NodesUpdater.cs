using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.Helpers;

public class NodesUpdater : IDisposable
{
    private readonly DirectoryViewModel treeRootViewModel;
    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly SelfSustainableTree model;
    private readonly Dispatcher dispatcher;
    private bool syncInProgress = false;

    public NodesUpdater(MainWindowViewModel mainWindowViewModel, SelfSustainableTree model, Dispatcher dispatcher)
    {
        if (mainWindowViewModel.Items == null || mainWindowViewModel.Items.Length != 1)
            throw new ArgumentException("Failed to start update on empty tree");
        
        treeRootViewModel = mainWindowViewModel.Items[0];
        this.mainWindowViewModel = mainWindowViewModel;
        this.model = model;
        this.dispatcher = dispatcher;

        var worker = new Thread(UpdateTree) { IsBackground = true };
        var timer = new Thread(UpdateTimer) { IsBackground = true };
        syncInProgress = true;
        worker.Start();
        timer.Start();
    }

    private void UpdateTimer()
    {
        dispatcher.Invoke(UpdateLoadingTime, mainWindowViewModel, model, TimeSpan.Zero);
        dispatcher.Invoke(UpdateLoaderVisibility, mainWindowViewModel, Visibility.Hidden);

        var stopWatch = Stopwatch.StartNew();
        do
        {
            if (stopWatch.Elapsed.TotalSeconds >= 1)
                dispatcher.Invoke(UpdateLoadingTime, mainWindowViewModel, model, stopWatch.Elapsed);
            Thread.Sleep(300);
        } while (syncInProgress && !model.Loaded);
        stopWatch.Stop();

        dispatcher.Invoke(UpdateLoaderVisibility, mainWindowViewModel, Visibility.Hidden);
    }

    private void UpdateTree()
    {
        while (syncInProgress)
        {
            var changedNodes = model.GetChangedNodes();

            var queue = new Queue<TreeViewItemViewModel>();
            queue.Enqueue(treeRootViewModel);
            while (queue.TryDequeue(out var current))
            {
                if (current.Entry == null)
                    continue;

                if (changedNodes.Contains(current.Entry))
                {
                    if (current.Loaded)
                        dispatcher.Invoke(UpdateNode, current, current.GetMissingChildren(), current.GetExtraChildren(),
                            model.Root.GetSize);
                    else
                        dispatcher.Invoke(UpdateSize, current, model.Root.GetSize);
                }
                else
                {
                    dispatcher.Invoke(UpdatePercentageFromRoot, current, model.Root.GetSize);
                }

                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
            
            Thread.Sleep(100);
        }
    }

    private static void UpdateLoaderVisibility(MainWindowViewModel mainWindowViewModel, Visibility visibility)
    {
        mainWindowViewModel.LoaderVisibility = visibility;
    }
    
    private static void UpdateLoadingTime(MainWindowViewModel mainWindowViewModel, SelfSustainableTree tree, TimeSpan loadingTime)
    {
        mainWindowViewModel.LoadingTime = loadingTime.ToString("mm\\:ss");
        mainWindowViewModel.LoaderVisibility = !tree.Loaded ? Visibility.Visible : Visibility.Hidden;
    }
    
    private static void UpdateSize(TreeViewItemViewModel current, long? rootSize)
    {
        current.UpdateSize();
        current.UpdatePercentFromRoot(rootSize);
    }

    private static void UpdatePercentageFromRoot(TreeViewItemViewModel current, long? rootSize)
    {
        current.UpdatePercentFromRoot(rootSize);
    }

    private static void UpdateNode(TreeViewItemViewModel node, IEnumerable<TreeViewItemViewModel> toAdd,
        IEnumerable<TreeViewItemViewModel> toDelete, long rootSize)
    {
        node.Update(toAdd, toDelete, rootSize);
    }
    
    public void Dispose()
    {
        syncInProgress = false;
        model.Dispose();
    }
}