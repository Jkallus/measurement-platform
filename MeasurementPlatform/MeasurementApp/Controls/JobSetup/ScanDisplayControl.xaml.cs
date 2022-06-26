using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Controls.RecipeSetup
{
    public sealed partial class ScanDisplayControl : UserControl
    {
        public ScanDisplayControlViewModel ViewModel { get; set; }
        public ScanDisplayControl()
        {
            ViewModel = App.GetService<ScanDisplayControlViewModel>();
            this.InitializeComponent();
        }
    }
}
