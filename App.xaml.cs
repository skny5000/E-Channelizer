using System;
using System.IO;
using System.Reflection;
using System.Windows;
using EChannelizer.Services;
using EChannelizer.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace EChannelizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static IConfiguration? Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Setup logging
            ConfigureLogging();

            // Handle unhandled exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Create and show main window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton(Configuration!);

            // Logging
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddNLog();
            });

            // Services
            services.AddSingleton<IRepositoryService, RepositoryService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IStreamService, StreamService>();
            services.AddSingleton<IImportExportService, ImportExportService>();
            services.AddSingleton<INetworkDeviceService, NetworkDeviceService>();
            services.AddSingleton<IEpgService, EpgService>();
            services.AddSingleton<ILogoService, LogoService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IUpdateService, UpdateService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ImportViewModel>();
            services.AddTransient<EditChannelViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ProfilesViewModel>();
            services.AddTransient<NetworkDevicesViewModel>();
            services.AddTransient<SatellitesViewModel>();
            services.AddTransient<MapSatellitesViewModel>();
            services.AddTransient<CreateBouquetsViewModel>();
            services.AddTransient<BatchEditEpgViewModel>();
            services.AddTransient<AssignEpgViewModel>();
            services.AddTransient<AssignLogosViewModel>();
            services.AddTransient<EditGroupViewModel>();
            services.AddTransient<EditTransponderViewModel>();
            services.AddTransient<EditSatelliteViewModel>();
            services.AddTransient<EditDeviceViewModel>();
            services.AddTransient<EditEpgSourceViewModel>();
            services.AddTransient<StreamPlayerViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<ImportWindow>();
            services.AddTransient<EditChannelWindow>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<ProfilesWindow>();
            services.AddTransient<NetworkDevicesWindow>();
            services.AddTransient<SatellitesWindow>();
            services.AddTransient<MapSatellitesWindow>();
            services.AddTransient<CreateBouquetsWindow>();
            services.AddTransient<BatchEditEpgWindow>();
            services.AddTransient<AssignEpgWindow>();
            services.AddTransient<AssignLogosWindow>();
            services.AddTransient<EditGroupWindow>();
            services.AddTransient<EditTransponderWindow>();
            services.AddTransient<EditSatelliteWindow>();
            services.AddTransient<EditDeviceWindow>();
            services.AddTransient<EditEpgSourceWindow>();
            services.AddTransient<StreamPlayerWindow>();
        }

        private void ConfigureLogging()
        {
            var logConfig = new NLog.Config.LoggingConfiguration();
            var logFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "E-Channelizer",
                "Logs",
                "e-channelizer.log");

            Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);

            var fileTarget = new NLog.Targets.FileTarget("file")
            {
                FileName = logFile,
                Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}",
                MaxArchiveFiles = 10,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Sequence,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                KeepFileOpen = false
            };

            var consoleTarget = new NLog.Targets.ConsoleTarget("console")
            {
                Layout = "${longdate} ${level:uppercase=true} ${logger} ${message}"
            };

            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

            NLog.LogManager.Configuration = logConfig;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = ServiceProvider?.GetService<ILogger<App>>();
            logger?.LogCritical(e.Exception, "Unhandled dispatcher exception");

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ServiceProvider?.GetService<ILogger<App>>();
            logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled domain exception");

            if (e.IsTerminating)
            {
                MessageBox.Show(
                    $"A fatal error occurred:\n\n{(e.ExceptionObject as Exception)?.Message}\n\nThe application will now exit.",
                    "Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            var logger = ServiceProvider?.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Unobserved task exception");
            e.SetObserved();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            NLog.LogManager.Shutdown();

            base.OnExit(e);
        }
    }
}