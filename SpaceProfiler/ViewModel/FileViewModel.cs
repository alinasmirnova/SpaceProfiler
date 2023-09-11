using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FileViewModel : TreeViewItemViewModel
{
    private readonly FileEntry entry;
    public FileViewModel(FileEntry entry) : base(entry, false)
    {
        this.entry = entry;
    }
    
    public string Name => entry.Name;
}