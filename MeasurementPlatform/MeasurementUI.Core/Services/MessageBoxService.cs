using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MeasurementUI.Core.Interfaces;

namespace MeasurementUI.Core.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessageBox(string message)
        {
            MessageBox.Show(message);
        }

        public bool ShowQuestionMessageBox(string message)
        {
            MessageBoxResult result = MessageBox.Show(message,"", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }
    }
}
