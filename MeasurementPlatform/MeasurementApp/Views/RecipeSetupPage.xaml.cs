using MeasurementApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Views
{
    
    public sealed partial class RecipeSetupPage : Page
    {
        public RecipeSetupViewModel ViewModel { get; set; }
        public RecipeSetupPage()
        {
            ViewModel = App.GetService<RecipeSetupViewModel>();
            this.InitializeComponent();
        }
    }
}
