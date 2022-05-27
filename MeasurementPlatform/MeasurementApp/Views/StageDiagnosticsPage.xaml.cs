using MeasurementApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Views
{
    public sealed partial class StageDiagnosticsPage : Page
    {
        public StageDiagnosticsViewModel ViewModel { get; }

        public StageDiagnosticsPage()
        {
            ViewModel = App.GetService<StageDiagnosticsViewModel>();
            InitializeComponent();
        }
    }
}
