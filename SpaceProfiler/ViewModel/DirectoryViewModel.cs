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

    public override void CompareChildren(out List<TreeViewItemViewModel> missingChildren, out List<TreeViewItemViewModel> extraChildren)
    {
        var subdirectories = GetSubdirectories();
        var files = GetFiles();
        
        var needsFilesContainer = NeedFilesContainer();
        if (filesContainer != null && IsExtraChild(filesContainer, files, subdirectories,  needsFilesContainer))
            filesContainer = null;
        
        extraChildren = Children.Where(child => IsExtraChild(child, files, subdirectories, needsFilesContainer)).ToList();
        
        missingChildren = new List<TreeViewItemViewModel>();
        if (needsFilesContainer && filesContainer == null)
        {
            filesContainer = new FilesContainerViewModel(Directory);
            missingChildren.Add(filesContainer);
        }
        
        if (!needsFilesContainer)
        {
            foreach (var file in files)
            {
                if (!ChildrenByEntry.ContainsKey(file))
                    missingChildren.Add(new FileViewModel(file));
            }
        }

        foreach (var subdirectory in subdirectories)
        {
            if (!ChildrenByEntry.ContainsKey(subdirectory))
                missingChildren.Add(new DirectoryViewModel(subdirectory));
        }
    }

    private static bool IsExtraChild(TreeViewItemViewModel child, HashSet<FileEntry> files, HashSet<DirectoryEntry> subdirectories, bool needsFilesContainer)
    {
        if (child is FilesContainerViewModel && !needsFilesContainer)
            return true;

        if (child is FilesContainerViewModel && needsFilesContainer)
            return false;

        if (child is FileViewModel && needsFilesContainer)
            return true;

        if (child is FileViewModel && !files.Contains(child.Entry))
            return true;
        
        if (child is DirectoryViewModel) 
            return !subdirectories.Contains(child.Entry);

        return false;
    }

    private bool NeedFilesContainer()
    {
        return Directory is { SubdirectoriesCount: > 0, FilesCount: > 1 } or { FilesCount: > MaxChildrenCount };
    }

    private HashSet<FileEntry> GetFiles()
    {
        if (Directory.FilesCount > MaxChildrenCount)
            return Directory.GetTopFiles(TopChildrenCount);
        return Directory.Files.ToHashSet();
    }
    
    private HashSet<DirectoryEntry> GetSubdirectories()
    {
        if (Directory.SubdirectoriesCount > MaxChildrenCount)
            return Directory.GetTopSubdirectories(TopChildrenCount);
        return Directory.Subdirectories.ToHashSet();
    }
}