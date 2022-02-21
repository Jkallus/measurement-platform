using MeasurementUI.Controls.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MeasurementUI.Controls.Converters
{
    public class StepSizeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((StepSize)parameter == (StepSize)value)
                return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value == true)
            {
                return (StepSize)parameter;
            }
            else return Binding.DoNothing;
        }
    }
}
