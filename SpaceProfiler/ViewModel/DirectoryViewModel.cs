using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SpaceProfiler.Helpers;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class DirectoryViewModel : TreeViewItemViewModel
{
    private DirectoryEntry Directory => (DirectoryEntry) Entry!;
    private FilesContainerViewModel? filesContainer;
    
    public DirectoryViewModel(DirectoryEntry entry) : base(entry, entry.SubdirectoriesCount > 0 || entry.FilesCount > 0)
    {
        Name = entry.Name;
        Count = entry.SubdirectoriesCount;
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
                if (value > MaxChildrenCount)
                    Name = $"{Entry?.Name} [{Count} subdirectories]";
            }
        }
    }

    public override void UpdateSize()
    {
        base.UpdateSize();
        Count = Directory.SubdirectoriesCount;
    }

    protected override void UpdateIcon()
    {
        if (!Directory.IsAccessible)
            Icon = Icons.Inaccessible;
        else
            Icon = IsExpanded ? Icons.OpenedDirectory : Icons.Directory;
    }

    protected override void LoadChildren()
    {
        foreach (var child in GetSubdirectories())
        {
            AddChild(new DirectoryViewModel(child));
        }

        if (NeedFilesContainer())
        {
            filesContainer = new FilesContainerViewModel(Directory);
            AddChild(filesContainer);
        }
        else
        {
            if (Directory.FilesCount > 0)
            {
                foreach (var file in GetFiles())
                {
                    AddChild(new FileViewModel(file));
                }
            }
        }
    }
    
    public override List<TreeViewItemViewModel> GetExtraChildren()
    {
        var needsFilesContainer = NeedFilesContainer();

        if (filesContainer != null && IsExtraChild(Directory, filesContainer, needsFilesContainer))
            filesContainer = null;
        
        return Children.Where(child => IsExtraChild(Directory, child, needsFilesContainer)).ToList();
    }

    private static bool IsExtraChild(DirectoryEntry directory, TreeViewItemViewModel child, bool needFilesContainer)
    {
        if (child is FilesContainerViewModel && !needFilesContainer)
            return true;

        if (child is FilesContainerViewModel && needFilesContainer)
            return false;

        if (child is FileViewModel && needFilesContainer)
            return true;

        if (child is FileViewModel && !directory.ContainsFile(child.Name))
            return true;
        
        if (child is DirectoryViewModel) 
            return !directory.ContainsSubdirectory(child.Entry!.Name);

        return false;
    }

    public override List<TreeViewItemViewModel> GetMissingChildren()
    {
        var result = new List<TreeViewItemViewModel>();
        if (NeedFilesContainer() && filesContainer == null)
        {
            filesContainer = new FilesContainerViewModel(Directory);
            result.Add(filesContainer);
        }
        
        if (!NeedFilesContainer())
        {
            foreach (var file in GetFiles())
            {
                if (!ChildrenByEntry.ContainsKey(file))
                    result.Add(new FileViewModel(file));
            }
        }

        foreach (var subdirectory in GetSubdirectories())
        {
            if (!ChildrenByEntry.ContainsKey(subdirectory))
                result.Add(new DirectoryViewModel(subdirectory));
        }

        return result;
    }

    private bool NeedFilesContainer()
    {
        return Directory is { SubdirectoriesCount: > 0, FilesCount: > 1 };
    }

    private FileEntry[] GetFiles()
    {
        if (Directory.FilesCount > MaxChildrenCount)
            return Directory.GetTopFiles(TopChildrenCount);
        return Directory.Files;
    }
    
    private DirectoryEntry[] GetSubdirectories()
    {
        if (Directory.SubdirectoriesCount > MaxChildrenCount)
            return Directory.GetTopSubdirectories(TopChildrenCount);
        return Directory.Subdirectories;
    }
}