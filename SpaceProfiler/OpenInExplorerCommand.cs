using System;
using System.Diagnostics;
using System.Windows.Input;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler;

public class OpenInExplorerCommand : ICommand
{
    private readonly FileSystemEntry? entry;

    public OpenInExplorerCommand(TreeViewItemViewModel viewModel)
    {
        entry = viewModel.Entry;
    }

    public bool CanExecute(object? parameter)
    {
        return entry is { IsAccessible: true };
    }

    public void Execute(object? parameter)
    {
        if (entry is DirectoryEntry)
            Start(entry.FullName);
        else if (entry is FileEntry)
            Start(entry.Parent?.FullName!);
    }

    private void Start(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    public event EventHandler? CanExecuteChanged;
}