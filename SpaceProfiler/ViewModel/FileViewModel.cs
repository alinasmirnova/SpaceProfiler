using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FileViewModel : TreeViewItemViewModel
{
    private readonly FileEntry entry;
    public FileViewModel(FileEntry entry, FileSystemEntry? root) : base(entry, root, false)
    {
        this.entry = entry;
    }
    
    public string Name => entry.Name;
}