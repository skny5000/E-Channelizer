using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for ProfilesWindow.xaml
    /// </summary>
    public partial class ProfilesWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ProfilesWindow(ProfilesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}