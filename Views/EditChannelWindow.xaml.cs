using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditChannelWindow.xaml
    /// </summary>
    public partial class EditChannelWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditChannelWindow(EditChannelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}