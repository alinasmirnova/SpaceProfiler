using System;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
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
        if (entry is { IsAccessible: false } or null)
            return false;

        if (entry is DirectoryEntry && !Directory.Exists(entry.FullName))
            return false;
        
        if (entry is FileEntry && (entry.Parent == null || !Directory.Exists(entry.Parent.FullName)))
            return false;

        return true;
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