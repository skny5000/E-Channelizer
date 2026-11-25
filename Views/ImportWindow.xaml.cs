using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ImportWindow(ImportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}