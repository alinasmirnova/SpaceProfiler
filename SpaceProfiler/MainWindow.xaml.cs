using System.Threading;
using System.Windows;
using Ookii.Dialogs.Wpf;
using SpaceProfiler.Helpers;
using SpaceProfiler.ViewModel;
using SpaceProfilerLogic;

namespace SpaceProfiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;
        private NodesUpdater? nodesUpdater;
        
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }
        
        private void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select a folder",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog()!.Value)
            {
                if (dialog.SelectedPath == viewModel.CurrentDirectory)
                    return;
                
                viewModel.CurrentDirectory = dialog.SelectedPath;
                nodesUpdater?.Dispose();
                
                var tree = new SelfSustainableTree(dialog.SelectedPath);
                viewModel.Items = new[] { new DirectoryViewModel(tree.Root) };
                nodesUpdater = new NodesUpdater(viewModel, tree, Dispatcher);
            }
        }
    }
}