using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private readonly DirectoryEntry entry;
    public DirectoryViewModel(DirectoryEntry entry) : base(entry.Subdirectories.Any() || entry.Files.Any())
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

        if (entry.Subdirectories.Any() && entry.Files.Count > 1)
        {
            Children?.Add(new FilesContainerViewModel(entry.Files));
        }
        else
        {
            foreach (var entryFile in entry.Files)
            {
                Children?.Add(new FileViewModel(entryFile));
            }
        }
    }
}