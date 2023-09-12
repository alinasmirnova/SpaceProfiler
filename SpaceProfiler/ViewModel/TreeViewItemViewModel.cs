﻿using System.Collections.Generic;
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

    private TreeViewItemViewModel() { }

    protected TreeViewItemViewModel(FileSystemEntry entry, bool hasChildren)
    {
        Children = new ObservableCollection<TreeViewItemViewModel>();
        if (hasChildren)
            Children.Add(UnloadedChild);

        Entry = entry;
        Size = FileSizeHelper.ToHumanReadableString(entry.GetSize);
    }

    public FileSystemEntry? Entry { get; }
    
    private string size;
    public string Size
    {
        get => size;
        set
        {
            if (value != size)
            {
                size = value;
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

    protected virtual void LoadChildren() {}
    protected virtual bool HasChildrenChanged() => false;
    protected virtual bool HasChildren() => false;
    public virtual void Update()
    {
        if (Entry == null)
            return;
        
        Size = FileSizeHelper.ToHumanReadableString(Entry.GetSize);
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