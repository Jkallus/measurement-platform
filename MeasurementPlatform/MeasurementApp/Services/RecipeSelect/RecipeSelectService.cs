using MeasurementUI.BusinessLogic.Recipe;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Services.RecipeSelect
{
    public class RecipeSelectService
    {        
        public RecipeSelectService()
        {
            
        }
        public async Task<ScanRecipe> SelectRecipe()
        {
            var dialog = new RecipeSelectContentDialog
            {
                Title = "Select Recipe",
                XamlRoot = App.MainRoot.XamlRoot,
                RequestedTheme = App.MainRoot.RequestedTheme,
                PrimaryButtonText = "Open",
                SecondaryButtonText = "Cancel"
            };

            var result = await dialog.ShowAsync();

            return dialog.Selection;
        }
    }
}
