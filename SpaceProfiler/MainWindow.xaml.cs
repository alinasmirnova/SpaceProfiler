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
                viewModel.CurrentDirectory = dialog.SelectedPath;
                tree?.StopSynchronization();

                tree = SelfSustainableTreeBuilder.Build(dialog.SelectedPath);
                tree.StartSynchronization();
                viewModel.Tree = new[] { new DirectoryViewModel(tree.Root) };
            }
        } 
    }
}