using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for NetworkDevicesWindow.xaml
    /// </summary>
    public partial class NetworkDevicesWindow : MahApps.Metro.Controls.MetroWindow
    {
        public NetworkDevicesWindow(NetworkDevicesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}