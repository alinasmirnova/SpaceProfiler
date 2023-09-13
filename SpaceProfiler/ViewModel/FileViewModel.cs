using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class FileViewModel : TreeViewItemViewModel
{
    private readonly FileEntry entry;
    public FileViewModel(FileEntry entry, FileSystemEntry? root) : base(entry, root, false)
    {
        this.entry = entry;
        SetIcon();
    }
    
    public string Name => entry.Name;

    protected override void OnSizeChanged()
    {
        SetIcon();
    }

    private void SetIcon()
    {
        Icon = GetSize() == 0 ? Icons.EmptyFile : Icons.File;
    }
}