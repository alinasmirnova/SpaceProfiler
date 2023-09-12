using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private DirectoryEntry Directory => (DirectoryEntry) Entry!;
    public DirectoryViewModel(DirectoryEntry entry, FileSystemEntry? root) : base(entry, root, entry.Subdirectories.Any() || entry.Files.Any())
    {
    }

    public string Name => Directory.Name;

    protected override void LoadChildren()
    {
        foreach (var child in Directory.Subdirectories)
        {
            Children?.Add(new DirectoryViewModel(child, Root));
        }

        if (Directory.Subdirectories.Any() && Directory.Files.Length > 1)
        {
            Children?.Add(new FilesContainerViewModel(Directory, Root));
        }
        else
        {
            foreach (var entryFile in Directory.Files)
            {
                Children?.Add(new FileViewModel(entryFile, Root));
            }
        }
    }

    protected override bool HasChildrenChanged()
    {
        if (Children == null)
            return false;
        
        var count = Directory.Subdirectories.Length;
        count += Directory.Files.Length > 1 ? 1 : Directory.Files.Length;
        if (count != Children.Count)
            return true;

        if (Directory.Subdirectories.Where((t, i) => Children[i].Entry != t).Any())
        {
            return true;
        }

        if (Directory.Files.Length > 0)
        {
            return Directory.Files.Length == 1 && Children.Last().Entry == Directory.Files[0] ||
                   Children.Last() is FilesContainerViewModel;
        }

        return false;
    }
    
    protected override bool HasChildren() => Directory.Subdirectories.Any() || Directory.Files.Any();
}