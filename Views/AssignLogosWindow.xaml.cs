using System.Windows;
using EChannelizer.ViewModels;

namespace EChannelizer.Views
{
    /// <summary>
    /// Interaction logic for AssignLogosWindow.xaml
    /// </summary>
    public partial class AssignLogosWindow : MahApps.Metro.Controls.MetroWindow
    {
        public AssignLogosWindow(AssignLogosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}