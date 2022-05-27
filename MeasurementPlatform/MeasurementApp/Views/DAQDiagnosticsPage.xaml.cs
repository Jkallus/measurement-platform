using MeasurementApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Views
{
    public sealed partial class DAQDiagnosticsPage : Page
    {
        public DAQDiagnosticsViewModel ViewModel { get; }

        public DAQDiagnosticsPage()
        {
            ViewModel = App.GetService<DAQDiagnosticsViewModel>();
            InitializeComponent();
        }
    }
}
