using SpaceProfilerLogic.Tree;

namespace SpaceProfiler.ViewModel;

public class MainWindowViewModel
{
    public DirectoryViewModel[]? Tree { get; }

    public MainWindowViewModel(FileSystemEntry? tree)
    {
        Tree = tree == null ? null : new [] {new DirectoryViewModel(tree)};
    }
}