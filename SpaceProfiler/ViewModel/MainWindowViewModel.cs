using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private string? currentDirectory;
    private DirectoryViewModel[]? items;
    private long rootSize = 0;

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