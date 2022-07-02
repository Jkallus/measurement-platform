using MeasurementApp.Services.RecipeSelect;
using MeasurementUI.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.JobRun
{
    public class JobRunControlViewModel: ObservableObject
    {
        // Private members
        private readonly IServiceProvider _service;
        private readonly ILogger _logger;
        private readonly RecipeSelectService _recipeSelect;

        // Public properties
        private ScanRecipe _recipe;
        public ScanRecipe Recipe
        {
            get => _recipe;
            set => SetProperty(ref _recipe, value);
        }

        public IAsyncRelayCommand SelectRecipeCommand { get; private set; }
        public IAsyncRelayCommand ExecuteRecipeCommand { get; private set; }

        public JobRunControlViewModel(IServiceProvider service, ILogger<JobRunControlViewModel> logger)
        {
            _service = service;
            _logger = logger;
            _recipeSelect = _service.GetService(typeof(RecipeSelectService)) as RecipeSelectService;
            SelectRecipeCommand = new AsyncRelayCommand(SelectRecipe);
            ExecuteRecipeCommand = new AsyncRelayCommand(ExecuteRecipe, CanExecuteRecipe);
        }

        private Task ExecuteRecipe()
        {
            throw new NotImplementedException();
        }

        private bool CanExecuteRecipe()
        {
            throw new NotImplementedException();
        }

        // Private methods
        private async Task SelectRecipe()
        {
            Recipe = await _recipeSelect.SelectRecipe();
        }
    }
}
