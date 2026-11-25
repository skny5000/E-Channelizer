using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for MapSatellitesWindow.xaml
    /// </summary>
    public partial class MapSatellitesWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MapSatellitesWindow(MapSatellitesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}