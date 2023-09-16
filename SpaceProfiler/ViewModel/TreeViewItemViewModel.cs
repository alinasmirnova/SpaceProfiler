using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SpaceProfiler.Helpers;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class TreeViewItemViewModel : INotifyPropertyChanged
{
    private static readonly TreeViewItemViewModel UnloadedChild = new();

    public ObservableCollection<TreeViewItemViewModel> Children { get; } = new();
    protected readonly Dictionary<FileSystemEntry, TreeViewItemViewModel> ChildrenByEntry = new();
    public bool Loaded => !(Children.Count == 1 && Children[0] == UnloadedChild);

    private TreeViewItemViewModel()
    {
        size = string.Empty;
        percentFromRoot = string.Empty;
        fontWeight = string.Empty;
        name = string.Empty;
        icon = string.Empty;
        opacity = CalculateOpacity();
    }

    protected TreeViewItemViewModel(FileSystemEntry entry, bool hasChildren) : this()
    {
        if (hasChildren)
            Children.Add(UnloadedChild);

        Entry = entry;
        UpdateFontWeight(0);
    }

    public FileSystemEntry? Entry { get; }
    
    private string size;
    public string Size
    {
        get => size;
        protected set
        {
            if (value != size)
            {
                size = value;
                UpdateIcon();
                OnPropertyChanged();
            }
        }
    }
    
    private long sizeValue;
    public long SizeValue
    {
        get => sizeValue;
        protected set
        {
            if (value != sizeValue)
            {
                sizeValue = value;
                UpdateIcon();
                OnPropertyChanged();
            }
        }
    }

    public virtual void UpdateSize()
    {
        SizeValue = Entry?.GetSize() ?? 0;
        Size = FileSizeHelper.ToHumanReadableString(SizeValue);
    }

    private string percentFromRoot;
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

    public virtual void UpdatePercentFromRoot(long? rootSize)
    {
        UpdatePercentFromRootInternal(FileSizeHelper.GetPercent(Entry?.GetSize(), rootSize));
    }

    protected void UpdatePercentFromRootInternal(double percent)
    {
        PercentFromRoot = FileSizeHelper.ToPercentString(percent);
        UpdateFontWeight(percent);
    }

    private bool isExpanded;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (value != isExpanded)
            {
                isExpanded = value;
                UpdateIcon();
                OnPropertyChanged();
            }

            if (!Loaded && value)
            {
                Children.RemoveAt(0);
                LoadChildren();
            }

            if (Loaded && !value && Children.Any())
            {
                Children.Clear();
                ChildrenByEntry.Clear();
                Children.Add(UnloadedChild);
            }
        }
    }
    
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (value != isSelected)
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    private string fontWeight;
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

    private void UpdateFontWeight(double percent) => FontWeight = percent > 0.2 ? "Bold" : "Normal";

    private string name;
    public string Name
    {
        get => name;
        protected set
        {
            if (value == name) return;
            name = value;
            OnPropertyChanged();
        }
    }

    private string icon;

    public string Icon
    {
        get => icon;
        protected set
        {
            if (value == icon) return;
            icon = value;
            OnPropertyChanged();
        }
    }

    private double opacity;
    public double Opacity
    {
        get => opacity;
        set
        {
            if (value.Equals(opacity)) return;
            opacity = value;
            OnPropertyChanged();
        }
    }
    
    private OpenInExplorerCommand? closeCommand;
    public ICommand CloseCommand => closeCommand ??= new OpenInExplorerCommand(this);

    protected virtual void UpdateIcon() { }

    protected virtual void LoadChildren() { }

    public virtual void Update(IEnumerable<TreeViewItemViewModel> toAdd, IEnumerable<TreeViewItemViewModel> toDelete, long? rootSize)
    {
        AddChildren(toAdd);
        RemoveChildren(toDelete);
        UpdateSize();
        UpdatePercentFromRoot(rootSize);
        Opacity = CalculateOpacity();
    }

    private double CalculateOpacity()
    {
        return Entry is {IsAccessible: false} ? 0.35 : 1;
    }

    private void AddChildren(IEnumerable<TreeViewItemViewModel> viewModels)
    {
        foreach (var viewModel in viewModels)
        {
            AddChild(viewModel);
        }
    }

    protected void AddChild(TreeViewItemViewModel viewModel)
    {
        viewModel.UpdateSize();
        viewModel.UpdateIcon();
        viewModel.Opacity = viewModel.CalculateOpacity();
        Children.Add(viewModel);
        ChildrenByEntry.Add(viewModel.Entry!, viewModel);
    }

    private void RemoveChildren(IEnumerable<TreeViewItemViewModel> viewModels)
    {
        foreach (var viewModel in viewModels)
        {
            Children.Remove(viewModel);
            ChildrenByEntry.Remove(viewModel.Entry!);
        }
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

    public virtual List<TreeViewItemViewModel> GetMissingChildren() => new();
    public virtual List<TreeViewItemViewModel> GetExtraChildren() => new();
}