using MeasurementApp.Core.Contracts.Services;
using System.Windows;

namespace MeasurementApp.Core.Services;

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
