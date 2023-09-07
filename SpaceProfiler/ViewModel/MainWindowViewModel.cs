using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private string? currentDirectory;
    private DirectoryViewModel[]? tree;

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

    public DirectoryViewModel[] Tree
    {
        get => tree;
        set
        {
            if (Equals(value, tree)) return;
            tree = value;
            OnPropertyChanged();
        }
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