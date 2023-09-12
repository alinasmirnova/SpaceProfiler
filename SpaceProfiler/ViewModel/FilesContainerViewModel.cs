using System.Collections.Generic;
using System.Linq;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FilesContainerViewModel : TreeViewItemViewModel
{
    private DirectoryEntry? Directory => (DirectoryEntry?)Entry;
    public FilesContainerViewModel(DirectoryEntry entry, FileSystemEntry? root) : base(entry, root, entry.Files.Any())
    {
        Count = entry.Files.Length;
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

    protected override bool HasChildrenChanged()
    {
        if (Children == null || Directory == null)
            return false;
        
        if (Children.Count != Directory.Files.Length)
            return true;

        return Children.Where((t, i) => t.Entry != Directory.Files[i]).Any();
    }
    
    protected override bool HasChildren() => Directory?.Files.Any() ?? false;

    public override void Update()
    {
        if (Directory == null)
            return;
        
        base.Update();
        Count = Directory.Files.Length;
    }

    protected override long GetSize()
    {
        if (Directory == null)
            return 0;
        
        return Directory.Files.Sum(f => f.GetSize);
    }

    protected override void LoadChildren()
    {
        if (Directory == null)
            return;
        
        foreach (var fileEntry in Directory.Files)
        {
            Children?.Add(new FileViewModel(fileEntry, Root));
        }
    }
}