using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.ViewModels
{
    public class ViewModelLocator
    {
        public SerialTerminalStubViewModel SerialTerminalStubViewModel
        {
            get
            {
                return Ioc.Default.GetService<SerialTerminalStubViewModel>();
            }
        }

    }
}
