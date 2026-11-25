using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditSatelliteWindow.xaml
    /// </summary>
    public partial class EditSatelliteWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditSatelliteWindow(EditSatelliteViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}