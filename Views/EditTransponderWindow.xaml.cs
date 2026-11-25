using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditTransponderWindow.xaml
    /// </summary>
    public partial class EditTransponderWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditTransponderWindow(EditTransponderViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}