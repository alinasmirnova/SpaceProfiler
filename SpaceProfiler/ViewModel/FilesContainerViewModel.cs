using System.Collections.Generic;
using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FilesContainerViewModel : TreeViewItemViewModel
{
    private readonly List<FileEntry> entries;
    public FilesContainerViewModel(List<FileEntry> entries) : base(entries.Any())
    {
        this.entries = entries;
    }

    public int Count => entries.Count;

    protected override void LoadChildren()
    {
        foreach (var child in entries)
        {
            Children?.Add(new FileViewModel(child));
        }
    }
}