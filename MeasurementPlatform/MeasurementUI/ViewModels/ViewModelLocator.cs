using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeasurementUI.Controls.ViewModels;

namespace MeasurementUI.ViewModels
{
    public class ViewModelLocator
    {
        public SerialTerminalControlStubViewModel SerialTerminalStubViewModel
        {
            get
            {
                return App.Current.Services.GetService(typeof(SerialTerminalControlStubViewModel)) as SerialTerminalControlStubViewModel;
                //return Ioc.Default.GetService<SerialTerminalControlStubViewModel>();
            }
        }

        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                return App.Current.Services.GetService(typeof(MainWindowViewModel)) as MainWindowViewModel;
                //return Ioc.Default.GetService<MainWindowViewModel>();
            }
        }

        public ConnectionControlStubViewModel ConnectionControlStubViewModel
        {
            get
            {
                return App.Current.Services.GetService(typeof(ConnectionControlStubViewModel)) as ConnectionControlStubViewModel;
                //return Ioc.Default.GetService<ConnectionControlStubViewModel>();
            }
        }

    }
}
