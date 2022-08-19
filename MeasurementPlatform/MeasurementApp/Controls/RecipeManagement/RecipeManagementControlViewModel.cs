using MeasurementApp.Contracts.Services;
using MeasurementApp.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
//using Microsoft.Toolkit.Mvvm.ComponentModel;
//using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementApp.BusinessLogic.Services.RecipeManager;

namespace MeasurementApp.Controls.RecipeManagement;

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

    //private ScanRecipe _selectedRecipe;
    //public ScanRecipe SelectedRecipe
    //{
    //    get => _selectedRecipe;
    //    set => SetProperty(ref _selectedRecipe, value);
    //}

    // Constructor
    public RecipeManagementControlViewModel(ILogger<RecipeManagementControlViewModel> logger, IServiceProvider service)
    {
        _logger = logger;
        _service = service;
        _recipeManager = _service.GetService(typeof(IRecipeManager)) as IRecipeManager ?? throw new Exception("IRecipeManager is null");
        _navigation = _service.GetService(typeof(INavigationService)) as INavigationService ?? throw new Exception("INavigationService is null");
        _recipes = new ObservableCollection<ScanRecipe>(_recipeManager.GetRecipes());
        _logger.LogInformation("RecipeManagementControlViewModel constructed");
        AddRecipeCommand = new RelayCommand(AddRecipe);
        RemoveRecipeCommand = new RelayCommand<ScanRecipe>(RemoveRecipe, CanRemoveRecipe);
        EditRecipeCommand = new RelayCommand<ScanRecipe>(EditRecipe, CanEditRecipe);
    }

    private void EditRecipe(ScanRecipe? obj)
    {
        if (obj != null)
        {
            _navigation.NavigateTo("MeasurementApp.ViewModels.RecipeSetupViewModel");
            WeakReferenceMessenger.Default.Send<EditRecipeMessage>(new EditRecipeMessage(obj));
        }
    }

    private bool CanEditRecipe(ScanRecipe? obj)
    {
        return obj != null;
    }

    private void RemoveRecipe(ScanRecipe? obj)
    {
        if(obj != null)
        {
            _recipeManager.RemoveRecipe(obj);
            Recipes = new(_recipeManager.GetRecipes());
        }
    }

    private bool CanRemoveRecipe(ScanRecipe? obj)
    {
        return obj != null;
    }

    private void AddRecipe()
    {
        _navigation.NavigateTo("MeasurementApp.ViewModels.RecipeSetupViewModel");
    }
}
