using MeasurementApp.BusinessLogic.Recipe;
using MeasurementApp.BusinessLogic.Services.RecipeManager;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Services.RecipeSelect;

public class RecipeSelectContentDialogViewModel : ObservableObject
{
    private readonly IServiceProvider _service;
    private readonly IRecipeManager _recipeManager;

    private ScanRecipe? _selection;
    public ScanRecipe? Selection // TODO check what happens when binding to null here
    {
        get => _selection; 
        set
        {
            if (SetProperty(ref _selection, value))
            {
                OnPropertyChanged("CanOpen");
            }
        }
    }

    public bool CanOpen => Selection != null;

    private ObservableCollection<ScanRecipe> _recipes;
    public ObservableCollection<ScanRecipe> Recipes
    {
        get => _recipes;
        set => SetProperty(ref _recipes, value);
    }


    public RecipeSelectContentDialogViewModel(IServiceProvider service)
    {
        _service = service;
        _recipeManager = _service.GetService(typeof(IRecipeManager)) as IRecipeManager ?? throw new Exception("Recipe manager is null");
        _recipes = new(_recipeManager.GetRecipes());
    }
}
