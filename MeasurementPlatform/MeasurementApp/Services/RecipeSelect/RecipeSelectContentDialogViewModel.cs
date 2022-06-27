using MeasurementUI.BusinessLogic.Recipe;
using MeasurementUI.BusinessLogic.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Services.RecipeSelect
{
    public class RecipeSelectContentDialogViewModel : ObservableObject
    {
        private readonly IServiceProvider _service;
        private readonly IRecipeManager _recipeManager;

        private ScanRecipe _selection;
        public ScanRecipe Selection
        {
            get { return _selection; }
            set { SetProperty(ref _selection, value); }
        }

        private ObservableCollection<ScanRecipe> _recipes;
        public ObservableCollection<ScanRecipe> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }


        public RecipeSelectContentDialogViewModel(IServiceProvider service)
        {
            _service = service;
            _recipeManager = _service.GetService(typeof(IRecipeManager)) as IRecipeManager;
            Recipes = new(_recipeManager.GetRecipes());
        }
    }
}
