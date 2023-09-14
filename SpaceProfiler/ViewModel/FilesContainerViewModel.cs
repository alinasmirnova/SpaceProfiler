using System.Collections.Generic;
using System.Linq;
using SpaceProfiler.Helpers;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FilesContainerViewModel : TreeViewItemViewModel
{
    private DirectoryEntry Directory => ((DirectoryEntry?)Entry)!;
    public FilesContainerViewModel(DirectoryEntry entry) : base(entry, entry.Files.Any())
    {
        Count = entry.Files.Length;
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
        foreach (var fileEntry in Directory.Files)
        {
            AddChild(new FileViewModel(fileEntry));
        }
    }

    public override void Update(IEnumerable<TreeViewItemViewModel> toAdd, IEnumerable<TreeViewItemViewModel> toDelete, long? rootSize)
    {
        base.Update(toAdd, toDelete, rootSize);
        Count = Directory.Files.Length;
    }

    public override void UpdateSize()
    {
        Size = FileSizeHelper.ToHumanReadableString(Directory.Files.Sum(f => f.GetSize));
    }

    public override void UpdatePercentFromRoot(long? rootSize)
    {
        UpdatePercentFromRootInternal(FileSizeHelper.GetPercent(Directory.Files.Sum(f => f.GetSize), rootSize));
    }

    public override List<TreeViewItemViewModel> GetExtraChildren()
    {
        return Children.Where(child => !Directory.Files.Contains(child.Entry)).ToList();
    }

    public override List<TreeViewItemViewModel> GetMissingChildren()
    {
        var result = new List<TreeViewItemViewModel>();

        foreach (var file in Directory.Files)
        {
            if (Children.All(child => child.Entry != file))
                result.Add(new FileViewModel(file));
        }
        
        return result;
    }
}