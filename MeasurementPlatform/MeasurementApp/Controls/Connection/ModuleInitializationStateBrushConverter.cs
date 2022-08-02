using MeasurementApp.BusinessLogic.SystemControl.Enums;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace MeasurementApp.Controls.Converters;

public class ModuleInitializationStateBrushConverter : IValueConverter
{
   

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        ModuleInitializationState status = (ModuleInitializationState)value;
        if (status == ModuleInitializationState.Uninitialized)
            return new SolidColorBrush(Colors.Red);
        else if (status == ModuleInitializationState.Initializing)
            return new SolidColorBrush(Colors.Orange);
        else if (status == ModuleInitializationState.Initialized)
            return new SolidColorBrush(Colors.Green);
        else
            throw new Exception("Invalid ModuleInitializationState value");
    }



    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
