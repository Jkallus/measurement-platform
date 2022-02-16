using MeasurementUI.BusinessLogic.Configuration;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.Controls.ViewModels;
using MeasurementUI.Controls.Views;
using MeasurementUI.Core.Interfaces;
using MeasurementUI.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MeasurementUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            Configuration = builder.Build();
            Services = ConfigureServices();

            


            var main = Services.GetService<MainWindow>();
            main.DataContext = Services.GetService<MainWindowViewModel>();
            main.Show();
        }

        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<SerialTerminalControlStubViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ConnectionControlStubViewModel>();
            
            services.AddSingleton<IDialogService,DialogService>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<MachineConfiguration>(MachineConfiguration.LoadConfiguration(Configuration.GetSection("MachineConfigurationLocation").Value));
            services.AddSingleton<ISystemController, SystemController>();

            DialogService.RegisterDialog<ConnectionControlStub, ConnectionControlStubViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
