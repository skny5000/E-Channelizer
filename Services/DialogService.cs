using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using EChannelizer.ViewModels;

namespace EChannelizer.Services
{
    /// <summary>
    /// Dialog service implementation using WPF dialogs
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly ILogger<DialogService> _logger;

        public DialogService(ILogger<DialogService> logger)
        {
            _logger = logger;
        }

        public async Task<bool?> ShowDialogAsync(ViewModelBase viewModel)
        {
            try
            {
                var window = CreateWindowForViewModel(viewModel);
                viewModel.RequestClose += (s, e) =>
                {
                    window.DialogResult = e.Result;
                    window.Close();
                };

                var result = window.ShowDialog();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show dialog");
                return false;
            }
        }

        private Window CreateWindowForViewModel(ViewModelBase viewModel)
        {
            // This would typically use a ViewLocator or DataTemplates
            // For now, create a basic window
            var window = new Window
            {
                Title = viewModel.Title,
                Content = viewModel, // Would need DataTemplate to render properly
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 400,
                MinHeight = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current?.MainWindow,
                ResizeMode = ResizeMode.CanResize
            };

            // Apply MahApps.Metro styling if available
            try
            {
                var metroWindowType = Type.GetType("MahApps.Metro.Controls.MetroWindow, MahApps.Metro");
                if (metroWindowType != null)
                {
                    var metroWindow = Activator.CreateInstance(metroWindowType) as Window;
                    if (metroWindow != null)
                    {
                        metroWindow.Title = viewModel.Title;
                        metroWindow.Content = viewModel;
                        metroWindow.SizeToContent = SizeToContent.WidthAndHeight;
                        metroWindow.MinWidth = 400;
                        metroWindow.MinHeight = 300;
                        metroWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        metroWindow.Owner = Application.Current?.MainWindow;
                        return metroWindow;
                    }
                }
            }
            catch
            {
                // Fall back to regular window
            }

            return window;
        }

        public Task<string> ShowOpenFileDialogAsync(string title, string filter)
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                Multiselect = false
            };

            var result = dialog.ShowDialog(Application.Current?.MainWindow);
            return Task.FromResult(result == true ? dialog.FileName : string.Empty);
        }

        public Task<string> ShowSaveFileDialogAsync(string title, string filter)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                OverwritePrompt = true
            };

            var result = dialog.ShowDialog(Application.Current?.MainWindow);
            return Task.FromResult(result == true ? dialog.FileName : string.Empty);
        }

        public Task<bool> ShowConfirmAsync(string title, string message)
        {
            var result = MessageBox.Show(
                Application.Current?.MainWindow,
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowErrorAsync(string title, string message)
        {
            MessageBox.Show(
                Application.Current?.MainWindow,
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string title, string message)
        {
            MessageBox.Show(
                Application.Current?.MainWindow,
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            MessageBox.Show(
                Application.Current?.MainWindow,
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return Task.CompletedTask;
        }

        public Task<T?> ShowInputDialogAsync<T>(string title, string message, T defaultValue = default!)
        {
            // Simple implementation - would need a proper input dialog
            var result = defaultValue;
            return Task.FromResult(result);
        }
    }
}