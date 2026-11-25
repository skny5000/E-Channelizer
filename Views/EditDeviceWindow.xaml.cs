using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditDeviceWindow.xaml
    /// </summary>
    public partial class EditDeviceWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditDeviceWindow(EditDeviceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}