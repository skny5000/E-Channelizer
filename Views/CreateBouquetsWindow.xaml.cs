using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for CreateBouquetsWindow.xaml
    /// </summary>
    public partial class CreateBouquetsWindow : MahApps.Metro.Controls.MetroWindow
    {
        public CreateBouquetsWindow(CreateBouquetsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}