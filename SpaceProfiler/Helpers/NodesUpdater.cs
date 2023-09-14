using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.Helpers;

public class NodesUpdater : IDisposable
{
    private readonly DirectoryViewModel viewModel;
    private readonly SelfSustainableTree model;
    private readonly Dispatcher dispatcher;
    private bool syncInProgress = false;

    public NodesUpdater(DirectoryViewModel viewModel, SelfSustainableTree model, Dispatcher dispatcher)
    {
        this.viewModel = viewModel;
        this.model = model;
        this.dispatcher = dispatcher;

        var worker = new Thread(UpdateTree) { IsBackground = true };
        syncInProgress = true;
        worker.Start();
    }

    private void UpdateTree()
    {
        while (syncInProgress)
        {
            var changedNodes = model.GetChangedNodes();

            var queue = new Queue<TreeViewItemViewModel>();
            queue.Enqueue(viewModel);
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