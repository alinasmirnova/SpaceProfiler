using System.Collections.Generic;
using System.Linq;
using SpaceProfiler.Helpers;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FilesContainerViewModel : TreeViewItemViewModel
{
    private DirectoryEntry Directory => ((DirectoryEntry?)Entry)!;
    public FilesContainerViewModel(DirectoryEntry entry) : base(entry, entry.FilesCount > 0)
    {
        Count = entry.FilesCount;
        Icon = Icons.Files;
    }

    private int count;
    public int Count
    {
        get => count;
        set
        {
            if (value != count)
            {
                count = value;
                Name = $"Files [{Count}]";
            }
        }
    }

    protected override void LoadChildren()
    {
        foreach (var fileEntry in GetFiles())
        {
            AddChild(new FileViewModel(fileEntry));
        }
    }

    public override void Update(IEnumerable<TreeViewItemViewModel> toAdd, IEnumerable<TreeViewItemViewModel> toDelete, long? rootSize)
    {
        base.Update(toAdd, toDelete, rootSize);
        Count = Directory.FilesCount;
    }

    public override void UpdateSize()
    {
        SizeValue = Directory.Files.Sum(f => f.GetSize());
        Size = FileSizeHelper.ToHumanReadableString(SizeValue);
    }

    public override void UpdatePercentFromRoot(long? rootSize)
    {
        UpdatePercentFromRootInternal(FileSizeHelper.GetPercent(Directory.Files.Sum(f => f.GetSize()), rootSize));
    }

    public override void CompareChildren(out List<TreeViewItemViewModel> missingChildren, out List<TreeViewItemViewModel> extraChildren)
    {
        var files = GetFiles();

        missingChildren = new List<TreeViewItemViewModel>();
        foreach (var file in files)
        {
            if (!ChildrenByEntry.ContainsKey(file))
                missingChildren.Add(new FileViewModel(file));
        }

        extraChildren = new List<TreeViewItemViewModel>();
        missingChildren = Children.Where(child => !files.Contains(child.Entry)).ToList();
    }

    private HashSet<FileEntry> GetFiles()
    {
        if (Directory.FilesCount > MaxChildrenCount)
            return Directory.GetTopFiles(TopChildrenCount);
        return Directory.Files.ToHashSet();
    }
}