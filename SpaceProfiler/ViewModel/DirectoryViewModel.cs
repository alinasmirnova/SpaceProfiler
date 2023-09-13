using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private DirectoryEntry Directory => (DirectoryEntry) Entry!;
    public DirectoryViewModel(DirectoryEntry entry, FileSystemEntry? root) : base(entry, root, entry.Subdirectories.Any() || entry.Files.Any())
    {
        SetIcon();
    }

    public string Name => Directory.Name;

    protected override void LoadChildren()
    {
        foreach (var child in Directory.Subdirectories)
        {
            Children?.Add(new DirectoryViewModel(child, Root));
        }

        if (NeedFilesContainer())
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

    protected override void OnExpandedChanged() => SetIcon();

    private void SetIcon()
    {
        Icon = IsExpanded ? Icons.OpenedDirectory : Icons.Directory;
    }

    protected override bool HasChildrenChanged()
    {
        if (Children == null)
            return false;
        
        var count = Directory.Subdirectories.Length;
        if (NeedFilesContainer())
            count++;
        else
        {
            count += Directory.Files.Length;
        }
        
        if (count != Children.Count)
            return true;

        for (var i = 0; i < Directory.Subdirectories.Length; i++)
        {
            if (Directory.Subdirectories[i] != Children[i].Entry)
                return true;
        }

        var current = Directory.Subdirectories.Length;
        if (NeedFilesContainer())
            return Children.Last() is not FilesContainerViewModel;

        for (var i = 0; i < Directory.Files.Length; i++)
        {
            if (Directory.Files[i] != Children[current + i].Entry)
                return true;
        }
        return false;
    }

    private bool NeedFilesContainer()
    {
        return Directory.Subdirectories.Any() && Directory.Files.Length > 1;
    }

    protected override bool HasChildren() => Directory.Subdirectories.Any() || Directory.Files.Any();
}