using MeasurementApp.BusinessLogic.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Services;

public interface IRecipeManager
{
    List<ScanRecipe> GetRecipes();
    void AddRecipe(ScanRecipe recipe);
    void RemoveRecipe(ScanRecipe recipe);
    void UpdateRecipe(ScanRecipe oldRecipe, ScanRecipe newRecipe);
}
