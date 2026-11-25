using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for AssignEpgWindow.xaml
    /// </summary>
    public partial class AssignEpgWindow : MahApps.Metro.Controls.MetroWindow
    {
        public AssignEpgWindow(AssignEpgViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}