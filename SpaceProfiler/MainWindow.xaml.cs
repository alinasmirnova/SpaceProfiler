using System.Windows;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic;

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
            var treeWatcher = FileSystemEntriesTreeBuilder.Build(currentDirectory);
            var viewModel = new MainWindowViewModel(treeWatcher.Tree);
            
            DataContext = viewModel;
        }
    }
}