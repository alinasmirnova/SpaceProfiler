using System.Windows;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic;
using SpaceProfilerLogic.Tree;

namespace SpaceProfiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var currentDirectory = "C:\\Users\\alina\\Pictures";
            var tree = FileSystemEntriesTreeBuilder.Build(currentDirectory);
            var viewModel = new MainWindowViewModel(tree);
            DataContext = viewModel;
        }
    }
}