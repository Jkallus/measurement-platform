using MeasurementApp.BusinessLogic.Recipe;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.RecipeManagement;

public class EditRecipeMessage
{
    public ScanRecipe Recipe { get; private set; }

    public EditRecipeMessage(ScanRecipe recipe)
    {
        Recipe = recipe;
    }
}
