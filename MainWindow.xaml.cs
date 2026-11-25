using System.Windows;
using System.Windows.Controls;
using EChannelizer.ViewModels;
using MahApps.Metro.Controls;

namespace EChannelizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                if (vm.Repository.IsModified)
                {
                    var result = MessageBox.Show(
                        "Repository has unsaved changes. Save before closing?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        await vm.SaveRepositoryCommand.ExecuteAsync(null);
                        if (vm.Repository.IsModified) // Save failed or cancelled
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                vm.Dispose();
            }
        }

        private void GroupsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm && e.NewValue is ChannelGroup group)
            {
                vm.SelectedGroup = group;
            }
        }

        private void ChannelsDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItems.Count == 0)
            {
                e.Handled = true;
            }
        }
    }
}