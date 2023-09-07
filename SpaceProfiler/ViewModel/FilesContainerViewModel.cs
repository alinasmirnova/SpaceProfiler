using System.Collections.Generic;
using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FilesContainerViewModel : TreeViewItemViewModel
{
    private readonly FileEntry[] entries;
    public FilesContainerViewModel(FileEntry[] entries) : base(entries.Any())
    {
        this.entries = entries;
    }

    public int Count => entries.Length;

    protected override void LoadChildren()
    {
        foreach (var child in entries)
        {
            Children?.Add(new FileViewModel(child));
        }
    }
}