using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private readonly DirectoryEntry entry;
    public DirectoryViewModel(DirectoryEntry entry) : base(entry.Subdirectories.Any())
    {
        this.entry = entry;
    }

    public string Name => entry.Name ?? entry.FullName;

    protected override void LoadChildren()
    {
        foreach (var child in entry.Subdirectories)
        {
            Children?.Add(new DirectoryViewModel(child));
        }
    }
}