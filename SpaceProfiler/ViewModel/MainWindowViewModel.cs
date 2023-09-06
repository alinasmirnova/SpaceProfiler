using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class MainWindowViewModel
{
    public DirectoryViewModel[]? Tree { get; }

    public MainWindowViewModel(FileSystemEntryTree? tree)
    {
        if (tree != null)
        {
            Tree = new[] { new DirectoryViewModel(tree.Root) };
            Tree[0].IsExpanded = true;
        }
    }
}