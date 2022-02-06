using MeasurementUI.Controls.ViewModels;
using MeasurementUI.Controls.Views;
using MeasurementUI.Core.Interfaces;
using MeasurementUI.Core.Services;
using MeasurementUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            Services = ConfigureServices();



            DialogService.RegisterDialog<ConnectionControlStub, ConnectionControlStubViewModel>();
        }

        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SerialTerminalControlStubViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ConnectionControlStubViewModel>();
            
            services.AddSingleton<IDialogService,DialogService>();
            
            return services.BuildServiceProvider();
        }
    }
}
