using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for SatellitesWindow.xaml
    /// </summary>
    public partial class SatellitesWindow : MahApps.Metro.Controls.MetroWindow
    {
        public SatellitesWindow(SatellitesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}