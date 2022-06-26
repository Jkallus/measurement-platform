using MeasurementApp.Core.Contracts.Services;
using MeasurementUI.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Services
{
    public class RecipeManager : IRecipeManager
    {
        // Private members
        IServiceProvider _service;
        ILogger<RecipeManager> _logger;
        IFileService _file;
        private List<ScanRecipe> _recipes;
        private string _storageDirectory;


        // Constructor
        public RecipeManager(IServiceProvider service, ILogger<RecipeManager> logger)
        {
            _service = service;
            _logger = logger;
            _file = _service.GetService(typeof(IFileService)) as IFileService ?? throw new ArgumentNullException("IFileService is null");
            _recipes = new List<ScanRecipe>();
            _storageDirectory = "C:\\temp\\data";
            LoadAllRecipes();
        }

        public void AddRecipe(ScanRecipe recipe)
        {
            _file.Save<ScanRecipe>(_storageDirectory, recipe.Name, recipe);
            _recipes.Add(recipe);
        }

        public List<ScanRecipe> GetRecipes()
        {
            return _recipes;
        }

        public void RemoveRecipe(ScanRecipe recipe)
        {
            _recipes.Remove(recipe);
            _file.Delete(_storageDirectory, recipe.Name);
        }

        // Private methods
        private void LoadAllRecipes()
        {
            if(!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
            string[] filenames = Directory.GetFiles(_storageDirectory);
            foreach(string file in filenames)
            {
                _recipes.Add(_file.Read<ScanRecipe>(_storageDirectory, file));
            }
        }
    }
}
