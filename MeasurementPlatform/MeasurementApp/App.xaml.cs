using MeasurementApp.Activation;
using MeasurementApp.Contracts.Services;
using MeasurementApp.Core.Contracts.Services;
using MeasurementApp.Core.Services;
using MeasurementApp.Helpers;
using MeasurementApp.Models;
using MeasurementApp.Services;
using MeasurementApp.ViewModels;
using MeasurementApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.IO;
using MeasurementUI.BusinessLogic.Configuration;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementApp.Controls;
using Serilog;
using Serilog.AspNetCore;
using Microsoft.Extensions.Logging.Configuration;
using System;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;
using MeasurementUI.Core.Models;
using DAQ.Interfaces;
using DAQ.Model;
using StageControl.Interfaces;
using StageControl.Model;
using MeasurementApp.Controls.JobSetup;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace MeasurementApp
{
    public partial class App : Application
    {
        // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration((builder) =>
            {
                builder.SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("machineconfig.json", optional: false, reloadOnChange: false)
                .Build();
            })
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                string[] cmdargs = Environment.GetCommandLineArgs();
                bool simulationMode = cmdargs.Length > 1 ? (cmdargs[1] == "Simulation") : false;

                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers

                // Application Custom Services
                services.AddSingleton<MachineConfiguration>(context.Configuration.GetSection("MachineConfig").Get<MachineConfiguration>());

                services.AddSingleton<StageSerialConfig>(context.Configuration.GetSection("MachineConfig:StageSerialConfig").Get<StageSerialConfig>());
                services.AddSingleton<DAQSerialConfig>(context.Configuration.GetSection("MachineConfig:DAQSerialConfig").Get<DAQSerialConfig>());
                services.AddSingleton<StageConfig>(context.Configuration.GetSection("MachineConfig:StageConfig").Get<StageConfig>());
                services.AddSingleton<SystemController>();

                services.AddSingleton(typeof(IDAQ), simulationMode ? typeof(ESPDAQSim) : typeof(ESPDAQ));
                services.AddSingleton(typeof(IMachineControl), simulationMode ? typeof(FNCMachineControlSim) : typeof(FNCMachineControl));

                // Services
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsServicePackaged>();
                services.AddTransient<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Core Services
                services.AddSingleton<IFileService, FileService>();

                // Views and ViewModels
                services.AddTransient<JobRunPage>();
                services.AddTransient<JobRunViewModel>();
                services.AddTransient<JobSetupViewModel>();
                services.AddTransient<JobSetupPage>();
                services.AddTransient<DAQDiagnosticsViewModel>();
                services.AddTransient<DAQDiagnosticsPage>();
                services.AddTransient<StageDiagnosticsViewModel>();
                services.AddTransient<StageDiagnosticsPage>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<HomePage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();
                

                services.AddTransient<ConnectionControlViewModel>();
                services.AddTransient<StagePositioningControlViewModel>();
                services.AddTransient<DAQDiagnosticsControlViewModel>();
                services.AddTransient<PositionReadoutControlViewModel>();
                services.AddTransient<StageGraphicalControlViewModel>();
                services.AddTransient<ExampleControlViewModel>();
                services.AddTransient<ScanDisplayControlViewModel>();
                services.AddTransient<ScanSettingsControlViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            })
            .Build();

        public static T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;

        public static Window MainWindow { get; set; } = new Window() { Title = "AppDisplayName".GetLocalized() };

        public static FrameworkElement MainRoot { get; private set; } // used to get a XamlRoot for content dialog extension methods

        public App()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Application Starting Up");
                InitializeComponent();
                UnhandledException += App_UnhandledException;
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "The application failed to start correctly");
                Log.CloseAndFlush();
            }
            finally
            {
                
                
            }

            
            
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO: Log and handle exceptions as appropriate.
            // For more details, see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs.
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            var activationService = App.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
            MainRoot = MainWindow.Content as FrameworkElement; // Content should be the shell page which is a framework element
        }
    }
}
