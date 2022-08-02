using MeasurementApp.BusinessLogic.Recipe;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MeasurementApp.Services.RecipeSelect;

public sealed partial class RecipeSelectContentDialog : ContentDialog
{
    public RecipeSelectContentDialogViewModel ViewModel { get; private set; }

    public ScanRecipe Selection => ViewModel.Selection;
    public RecipeSelectContentDialog()
    {
        ViewModel = App.GetService<RecipeSelectContentDialogViewModel>();
        this.InitializeComponent();
    }
}
