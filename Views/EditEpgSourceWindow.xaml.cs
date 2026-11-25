using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditEpgSourceWindow.xaml
    /// </summary>
    public partial class EditEpgSourceWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditEpgSourceWindow(EditEpgSourceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}