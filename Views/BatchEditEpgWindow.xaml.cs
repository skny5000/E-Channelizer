using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for BatchEditEpgWindow.xaml
    /// </summary>
    public partial class BatchEditEpgWindow : MahApps.Metro.Controls.MetroWindow
    {
        public BatchEditEpgWindow(BatchEditEpgViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}