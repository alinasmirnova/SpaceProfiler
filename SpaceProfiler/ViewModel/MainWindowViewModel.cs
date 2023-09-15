using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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

    private Visibility loaderVisibility = Visibility.Hidden;

    public Visibility LoaderVisibility
    {
        get => loaderVisibility;
        set
        {
            if (value == loaderVisibility) return;
            loaderVisibility = value;
            OnPropertyChanged();
        }
    }

    private string loadingTime = string.Empty;
    public string LoadingTime
    {
        get => loadingTime;
        set
        {
            if (value == "00:00")
                value = string.Empty;

            if (value == loadingTime) return;
            loadingTime = value;
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