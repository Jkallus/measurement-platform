using MeasurementApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogViewerPage : Page
    {
        public LogViewerViewModel ViewModel { get; set; }
        public LogViewerPage()
        {
            ViewModel = App.GetService<LogViewerViewModel>();
            this.InitializeComponent();
        }
    }
}
