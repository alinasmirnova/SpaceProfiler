using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpaceProfiler.Helpers;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class TreeViewItemViewModel : INotifyPropertyChanged
{
    private static readonly TreeViewItemViewModel UnloadedChild = new();

    public ObservableCollection<TreeViewItemViewModel>? Children { get; }

    private TreeViewItemViewModel()
    {
        size = string.Empty;
        percentFromRoot = string.Empty;
    }

    protected TreeViewItemViewModel(FileSystemEntry entry, FileSystemEntry? root, bool hasChildren)
    {
        Children = new ObservableCollection<TreeViewItemViewModel>();
        if (hasChildren)
            Children.Add(UnloadedChild);

        Entry = entry;
        Root = root;
        SizeValue = GetSize();
    }

    public FileSystemEntry? Entry { get; }
    public FileSystemEntry? Root { get; }
    
    private long? sizeValue;
    public long? SizeValue
    {
        get => sizeValue ?? 0;
        set
        {
            if (value == sizeValue || value == null) return;
            sizeValue = value;
            var percent = FileSizeHelper.GetPercent(value.Value, Root?.GetSize);
            PercentFromRoot = $"{percent:P}";
            Size = FileSizeHelper.ToHumanReadableString(value.Value);
            FontWeight = percent > 0.1 ? "Bold" : "Normal";
        }
    }

    private string size = null!;
    public string Size
    {
        get => size;
        private set
        {
            if (value != size)
            {
                size = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string percentFromRoot = null!;
    public string PercentFromRoot
    {
        get => percentFromRoot;
        private set
        {
            if (value != percentFromRoot)
            {
                percentFromRoot = value;
                OnPropertyChanged();
            }
        }
    }

    public bool NotFullyLoaded => Children?.Count == 1 && Children[0] == UnloadedChild;

    private bool isExpanded;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (value != isExpanded)
            {
                isExpanded = value;
                OnPropertyChanged();
            }

            if (NotFullyLoaded)
            {
                Children?.RemoveAt(0);
                LoadChildren();
            }
        }
    }

    private string fontWeight = null!;

    public string FontWeight
    {
        get => fontWeight;
        set
        {
            if (value != fontWeight)
            {
                fontWeight = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void LoadChildren() {}
    protected virtual bool HasChildrenChanged() => false;
    protected virtual bool HasChildren() => false;
    protected virtual long GetSize() => Entry?.GetSize ?? 0;
    public virtual void Update()
    {
        if (Entry == null)
            return;
        
        SizeValue = GetSize();
        if (HasChildren() && NotFullyLoaded)
            return;

        if (!HasChildren() && NotFullyLoaded)
        {
            Children?.Clear();
            return;
        }
        
        if (!HasChildrenChanged()) return;
        
        IsExpanded = false;
        Children?.Clear();
        if (HasChildren())
            Children?.Add(UnloadedChild);
    }
    
    #region INotifyPropertyChanged Members
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}