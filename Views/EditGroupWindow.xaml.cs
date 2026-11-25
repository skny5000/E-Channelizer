using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for EditGroupWindow.xaml
    /// </summary>
    public partial class EditGroupWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EditGroupWindow(EditGroupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}