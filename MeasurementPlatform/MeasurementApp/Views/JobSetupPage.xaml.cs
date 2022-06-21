using MeasurementApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Views
{
    
    public sealed partial class JobSetupPage : Page
    {
        public JobSetupViewModel ViewModel { get; set; }
        public JobSetupPage()
        {
            ViewModel = App.GetService<JobSetupViewModel>();
            this.InitializeComponent();
        }
    }
}
