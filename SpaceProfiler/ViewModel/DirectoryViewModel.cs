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
    
    public DirectoryViewModel(DirectoryEntry entry) : base(entry, entry.Subdirectories.Any() || entry.Files.Any())
    {
        Name = entry.Name;
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
        foreach (var child in Directory.Subdirectories)
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
            if (Directory.Files.Any())
            {
                foreach (var file in Directory.Files)
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
            return !directory.ContainsSubdirectory(child.Name);

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
            foreach (var file in Directory.Files)
            {
                if (!ChildrenByEntry.ContainsKey(file))
                    result.Add(new FileViewModel(file));
            }
        }

        foreach (var subdirectory in Directory.Subdirectories)
        {
            if (!ChildrenByEntry.ContainsKey(subdirectory))
                result.Add(new DirectoryViewModel(subdirectory));
        }

        return result;
    }

    private bool NeedFilesContainer()
    {
        return Directory.Subdirectories.Any() && Directory.Files.Length > 1;
    }
}