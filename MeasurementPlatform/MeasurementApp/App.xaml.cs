using System.Diagnostics;
using DAQ.Interfaces;
using DAQ.Model;
using MeasurementApp.Activation;
using MeasurementApp.BusinessLogic.Configuration;
using MeasurementApp.BusinessLogic.Services;
using MeasurementApp.BusinessLogic.SystemControl;
using MeasurementApp.Contracts.Services;
using MeasurementApp.Controls;
using MeasurementApp.Controls.JobRun;
using MeasurementApp.Controls.RecipeManagement;
using MeasurementApp.Controls.RecipeSetup;
using MeasurementApp.Core.Contracts.Services;
using MeasurementApp.Core.Models;
using MeasurementApp.Core.Services;
using MeasurementApp.Helpers;
using MeasurementApp.Models;
using MeasurementApp.Services;
using MeasurementApp.Services.RecipeSelect;
using MeasurementApp.ViewModels;
using MeasurementApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;
using StageControl.Interfaces;
using StageControl.Model;

namespace MeasurementApp;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    public static FrameworkElement MainRoot { get; private set; } // used to get a XamlRoot for content dialog extension methods

    public App()
    {
        InitializeComponent();
        InitializeSetup();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        ConfigureAppConfiguration((builder) =>
        {
            string? basePath = Environment.GetEnvironmentVariable("MEASUREAPP_DIR");
            builder.SetBasePath(basePath)
            .AddJsonFile("settings\\machineconfig.json", optional: false, reloadOnChange: false)
            .Build();
        }).
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            string[] cmdargs = Environment.GetCommandLineArgs();
            bool simulationMode = cmdargs.Length > 1 ? (cmdargs[1] == "Simulation") : false;

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Application Custom Services
            services.AddSingleton<MachineConfiguration>(context.Configuration.GetSection("MachineConfig").Get<MachineConfiguration>());
            services.AddSingleton<IRecipeManager, RecipeManager>();
            services.AddSingleton<JobRunner>();
            services.AddSingleton<RecipeSelectService>();
            services.AddSingleton<StageSerialConfig>(context.Configuration.GetSection("MachineConfig:StageSerialConfig").Get<StageSerialConfig>());
            services.AddSingleton<DAQSerialConfig>(context.Configuration.GetSection("MachineConfig:DAQSerialConfig").Get<DAQSerialConfig>());
            services.AddSingleton<StageConfig>(context.Configuration.GetSection("MachineConfig:StageConfig").Get<StageConfig>());
            services.AddSingleton<SystemController>();
            services.AddSingleton(typeof(IDAQ), simulationMode ? typeof(ESPDAQSim) : typeof(ESPDAQ));
            services.AddSingleton(typeof(IMachineControl), simulationMode ? typeof(FNCMachineControlSim) : typeof(FNCMachineControl));

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<RecipeManagerPage>();
            services.AddTransient<RecipeManagerViewModel>();
            services.AddTransient<LogViewerPage>();
            services.AddTransient<LogViewerViewModel>();
            services.AddTransient<JobRunPage>();
            services.AddTransient<JobRunViewModel>();
            services.AddTransient<RecipeSetupViewModel>();
            services.AddTransient<RecipeSetupPage>();
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
            services.AddTransient<RecipeManagementControlViewModel>();
            services.AddTransient<ExampleControlViewModel>();
            services.AddSingleton<JobRunControlViewModel>();
            services.AddTransient<RecipeSelectContentDialogViewModel>();

            services.AddSingleton<ScanDisplayControlViewModel>();
            services.AddSingleton<StageGraphicalControlViewModel>();
            services.AddSingleton<ScanSettingsControlViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void InitializeSetup()
    {
        bool logInit = false;
        try
        {
            string? measureAppDir = Environment.GetEnvironmentVariable("MEASUREAPP_DIR");
            if (string.IsNullOrEmpty(measureAppDir))
            {
                throw new Exception("Environment variable MEASUREAPP_DIR null or empty");
            }

            if (!Directory.Exists(measureAppDir))
            {
                throw new Exception($"MeasureApp directory ({measureAppDir}) does not exist");
            }

            string appsettingsPath = Path.Combine(measureAppDir, "settings\\appsettings.json");
            if (!File.Exists(appsettingsPath))
            {
                throw new Exception($"appsettings.json does not exist at: {appsettingsPath}");
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            logInit = true;

            Log.Information("Application Starting Up");
            Log.Information($"Running out of {Directory.GetCurrentDirectory()}");

            var process = Process.GetCurrentProcess();
            string fullPath = process.MainModule!.FileName!;
            Log.Information($"Launched out of {fullPath}");

            UnhandledException += App_UnhandledException;

            using (Process p = Process.GetCurrentProcess())
            {
                p.PriorityClass = ProcessPriorityClass.High;
            }
        }
        catch (Exception ex)
        {
            if (logInit)
            {
                Log.Fatal(ex, "The application failed to start correctly");
                Log.CloseAndFlush();
                Environment.Exit(-1);
            }
            else
            {
                Environment.Exit(-1);
            }
        }
    }


    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception caught in App_UnhandledException: {Message}", e.Message);
        Environment.Exit(-1);
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
        MainRoot = MainWindow.Content as FrameworkElement ?? throw new Exception("MainWindow.Content is null");
    }
}
