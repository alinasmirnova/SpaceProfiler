using System.Threading;
using System.Windows;
using Ookii.Dialogs.Wpf;
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
        private SelfSustainableTree? tree;
        private bool syncInProgress = false;
        private readonly Thread background;
        
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            background = new Thread(UpdateTree) { IsBackground = true };
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
                tree?.Dispose();
                
                tree = new SelfSustainableTree(dialog.SelectedPath);
                if (tree?.Root != null)
                {
                    viewModel.Items = new[] { new DirectoryViewModel(tree.Root, tree.Root) };
                    if (!syncInProgress)
                    {
                        background.Start();
                        syncInProgress = true;
                    }
                }
            }
        }

        private void UpdateTree()
        {
            while (syncInProgress)
            {
                if (tree != null)
                {
                    var changes = tree.GetChangedNodes();
                    var changedNodes = viewModel.GetNodesForUpdate(changes);
                    foreach (var node in changedNodes)
                    {
                        Dispatcher.Invoke(() => { node.Update(); });
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}