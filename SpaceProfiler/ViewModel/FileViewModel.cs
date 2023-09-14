using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FileViewModel : TreeViewItemViewModel
{
    private readonly FileEntry entry;
    public FileViewModel(FileEntry entry) : base(entry, false)
    {
        this.entry = entry;
        Name = entry.Name;
    }

    protected override void UpdateIcon()
    {
        Icon = entry.GetSize == 0 ? Icons.EmptyFile : Icons.File;
    }
}