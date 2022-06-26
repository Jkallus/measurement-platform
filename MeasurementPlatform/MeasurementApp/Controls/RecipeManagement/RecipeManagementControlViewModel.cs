using MeasurementApp.Contracts.Services;
using MeasurementUI.BusinessLogic.Recipe;
using MeasurementUI.BusinessLogic.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.RecipeManagement
{
    public class RecipeManagementControlViewModel: ObservableObject
    {
        // Private member variables
        private readonly ILogger<RecipeManagementControlViewModel> _logger;
        private readonly IServiceProvider _service;
        private readonly IRecipeManager _recipeManager;
        private readonly INavigationService _navigation;

        // Public properties
        private ObservableCollection<ScanRecipe> _recipes;
        public ObservableCollection<ScanRecipe> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        public RelayCommand AddRecipeCommand { get; private set; }
        public RelayCommand<ScanRecipe> RemoveRecipeCommand { get; private set; }
        public RelayCommand<ScanRecipe> EditRecipeCommand { get; private set; }

        // Constructor
        public RecipeManagementControlViewModel(ILogger<RecipeManagementControlViewModel> logger, IServiceProvider service)
        {
            _logger = logger;
            _service = service;
            _recipeManager = _service.GetService(typeof(IRecipeManager)) as IRecipeManager;
            _navigation = _service.GetService(typeof(INavigationService)) as INavigationService;
            Recipes = new ObservableCollection<ScanRecipe>(_recipeManager.GetRecipes());
            _logger.LogInformation("RecipeManagementControlViewModel constructed");
            AddRecipeCommand = new RelayCommand(AddRecipe);
            RemoveRecipeCommand = new RelayCommand<ScanRecipe>(RemoveRecipe, CanRemoveRecipe);
            EditRecipeCommand = new RelayCommand<ScanRecipe>(EditRecipe, CanEditRecipe);
        }

        private void EditRecipe(ScanRecipe obj)
        {
            
        }

        private bool CanEditRecipe(ScanRecipe obj)
        {
            return false;
        }

        private void RemoveRecipe(ScanRecipe obj)
        {
            
        }

        private bool CanRemoveRecipe(ScanRecipe obj)
        {
            return false;
        }

        private void AddRecipe()
        {
            _navigation.NavigateTo("MeasurementApp.ViewModels.RecipeSetupViewModel");
        }
    }
}
