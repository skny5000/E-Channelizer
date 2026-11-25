using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for StreamPlayerWindow.xaml
    /// </summary>
    public partial class StreamPlayerWindow : MahApps.Metro.Controls.MetroWindow
    {
        public StreamPlayerWindow(StreamPlayerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is StreamPlayerViewModel vm)
            {
                vm.ClosePlayerCommand.Execute(null);
            }
        }
    }
}