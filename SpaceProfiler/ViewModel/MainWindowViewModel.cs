using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private string? currentDirectory;
    private DirectoryViewModel[]? items;

    public string? CurrentDirectory
    {
        get => currentDirectory;
        set
        {
            if (value == currentDirectory) return;
            currentDirectory = value;
            OnPropertyChanged();
        }
    }
    
    public DirectoryViewModel[]? Items
    {
        get => items;
        set
        {
            if (Equals(value, items)) return;
            items = value;
            OnPropertyChanged();
        }
    }
    
    public List<TreeViewItemViewModel> GetNodesForUpdate(HashSet<FileSystemEntry> changes)
    {
        var result = new List<TreeViewItemViewModel>();
        if (items == null || items.Length == 0)
            return result;

        if (changes.Count == 0)
            return result;

        var queue = new Queue<TreeViewItemViewModel>();
        queue.Enqueue(items[0]);
        while (queue.TryDequeue(out var current))
        {
            if (current.Entry != null && changes.Contains(current.Entry))
                result.Add(current);
            
            if (current.NotFullyLoaded || current.Children == null)
                continue;
            
            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}