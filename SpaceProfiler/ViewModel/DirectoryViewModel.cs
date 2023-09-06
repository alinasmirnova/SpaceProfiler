using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private readonly FileSystemEntry entry;
    public DirectoryViewModel(FileSystemEntry entry) : base(entry.Children.Any())
    {
        this.entry = entry;
    }

    public string Name => entry.Name ?? entry.FullName;

    protected override void LoadChildren()
    {
        foreach (var child in entry.Children)
        {
            Children?.Add(new DirectoryViewModel(child));
        }
    }
}