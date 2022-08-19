using MeasurementApp.Core.Contracts.Services;
using MeasurementApp.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Services.RecipeManager;

public class FileRecipeManager : IRecipeManager
{
    // Private members
    IServiceProvider _service;
    ILogger<FileRecipeManager> _logger;
    IFileService _file;
    private List<ScanRecipe> _recipes;
    private string _storageDirectory;


    // Constructor
    public FileRecipeManager(IServiceProvider service, ILogger<FileRecipeManager> logger)
    {
        _service = service;
        _logger = logger;
        _file = _service.GetService(typeof(IFileService)) as IFileService ?? throw new ArgumentNullException("IFileService is null");
        _recipes = new List<ScanRecipe>();
        _storageDirectory = Path.Combine(Environment.GetEnvironmentVariable("MEASUREAPP_DIR")!, "recipes\\");
        LoadAllRecipes();
        _logger.LogInformation("Recipe Manager constructed");
    }

    public void AddRecipe(ScanRecipe recipe)
    {
        _file.Save(_storageDirectory, recipe.Name, recipe);
        _recipes.Add(recipe);
        _logger.LogInformation($"Recipe {recipe.Name} added");
    }

    public List<ScanRecipe> GetRecipes()
    {
        return _recipes;
    }

    public void RemoveRecipe(ScanRecipe recipe)
    {
        _recipes.Remove(recipe);
        _file.Delete(_storageDirectory, recipe.Name);
        _logger.LogInformation($"Recipe {recipe.Name} removed");
    }

    public void UpdateRecipe(ScanRecipe oldRecipe, ScanRecipe newRecipe)
    {
        RemoveRecipe(oldRecipe);
        AddRecipe(newRecipe);
    }

    // Private methods
    private void LoadAllRecipes()
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
        List<string> filenames = Directory.GetFiles(_storageDirectory).Select(file => Path.GetFileName(file)).ToList();
        foreach (string file in filenames)
        {
            _recipes.Add(_file.Read<ScanRecipe>(_storageDirectory, file));
        }
        _logger.LogInformation($"Loaded {filenames.Count} recipes from storage");
    }
}