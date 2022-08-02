using MeasurementApp.Controls.RecipeManagement;
using MeasurementApp.Core.Models;
using MeasurementApp.BusinessLogic.Recipe;
using MeasurementApp.BusinessLogic.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.RecipeSetup;

public class ScanSettingsControlViewModel: ObservableRecipient
{
    // Private member variables
    private readonly ILogger<ScanSettingsControlViewModel> _logger;
    private readonly IServiceProvider _service;
    private readonly IRecipeManager _recipeManager;
    private bool _dataReceived; // have we received data from the ScanDisplayControl yet or is it still populated with default values
    private bool _isEditing;
    private ScanRecipe? _oldRecipe;

    // Public properties
    private string _recipeName;
    public string RecipeName
    {
        get { return _recipeName; }
        set 
        {
            if(SetProperty(ref _recipeName, value))
            {
                SaveRecipeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private PositionCoordinate _bottomLeft;
    public PositionCoordinate BottomLeft
    {
        get => _bottomLeft;
        set => SetProperty(ref _bottomLeft, value);
    }

    private PositionCoordinate _topLeft;
    public PositionCoordinate TopLeft
    {
        get => _topLeft;
        set => SetProperty(ref _topLeft, value);
    }

    private PositionCoordinate _topRight;
    public PositionCoordinate TopRight
    {
        get => _topRight;
        set => SetProperty(ref _topRight, value);
    }

    private PositionCoordinate _bottomRight;
    public PositionCoordinate BottomRight
    {
        get => _bottomRight;
        set => SetProperty(ref _bottomRight, value);
    }

    public ScanDimension XDimension
    {
        get => new ScanDimension(BottomRight.X - BottomLeft.X, Units.Millimeters);
    }

    public ScanDimension YDimension
    {
        get => new ScanDimension(TopLeft.Y - BottomLeft.Y, Units.Millimeters);
    }

    public ScanDimension ScanArea
    {
        get => new ScanDimension(XDimension.Value * YDimension.Value, Units.SquareMillimeters);
    }


    private double _sliderValue;
    public double SliderValue
    {
        get => _sliderValue;
        set
        {
            if(SetProperty(ref _sliderValue, value))
            {
                double adjustedValue = 100 + (0.01 - 100) / (1 + Math.Pow((value / 1.883328), 3.472488));
                adjustedValue *= 1000; // convert to um
                ScanPitch = new ScanDimension(adjustedValue, Units.Micrometers);
                UpdateCalculations();
            }
        }
    }

    /*
     * Scan Pitch Notes:
     * Scan pitch can range from 0.01mm to 10mm
     * Linear slider maps to exponential scale
     * 0 -> 0.01
     * 0.25 -> 0.1
     * 0.5 -> 1
     * 1.0 -> 10
     * 
     * Y = 100 + (0.01 - 100)/(1 + (X/1.883328)^3.472488) from https://mycurvefit.com/
     */


    private ScanDimension _scanPitch;
    public ScanDimension ScanPitch
    {
        get => _scanPitch;
        set => SetProperty(ref _scanPitch, value);
    }

    private int _xSampleCount;
    public int XSampleCount
    {
        get => _xSampleCount;
        set => SetProperty(ref _xSampleCount, value);
    }

    private int _ySampleCount;
    public int YSampleCount
    {
        get => _ySampleCount;
        set => SetProperty(ref _ySampleCount, value);
    }
    
    public int TotalSampleCount
    {
        get => XSampleCount * YSampleCount;
    }

    public RelayCommand SaveRecipeCommand { get; private set; }

    // Constructor
    public ScanSettingsControlViewModel(IServiceProvider service, ILogger<ScanSettingsControlViewModel> logger)
    {
        _service = service;
        _logger = logger;
        _recipeManager = _service.GetService(typeof(IRecipeManager)) as IRecipeManager ?? throw new Exception("IRecipeManager is null");
        _bottomLeft = new PositionCoordinate(0, 0);
        _topLeft = new PositionCoordinate(0, 0);
        _topRight = new PositionCoordinate(0, 0);
        _bottomRight = new PositionCoordinate(0, 0);
        _scanPitch = new ScanDimension(1000, Units.Micrometers);
        _sliderValue = 0.5;
        _recipeName = "";
        _dataReceived = false;
        _isEditing = false;
        _oldRecipe = null;
        SaveRecipeCommand = new RelayCommand(SaveRecipe, CanSaveRecipe);

        WeakReferenceMessenger.Default.Register<ScanAreaSelectionMessage>(this, (r, m) =>
        {
            App.MainRoot.DispatcherQueue.TryEnqueue(() =>
            {
                BottomLeft = m.Value.bottomLeft;
                TopLeft = m.Value.topLeft;
                TopRight = m.Value.topRight;
                BottomRight = m.Value.bottomRight;
                if(!_dataReceived)
                {
                    _dataReceived = true;
                    SaveRecipeCommand.NotifyCanExecuteChanged();
                }
                UpdateCalculations();
                
            });
        });

        WeakReferenceMessenger.Default.Register<EditRecipeMessage>(this, (r, m) =>
        {
            App.MainRoot.DispatcherQueue.TryEnqueue(() =>
            {
                BottomLeft = m.Recipe.BottomLeft;
                TopLeft = m.Recipe.TopLeft;
                TopRight = m.Recipe.TopRight;
                BottomRight = m.Recipe.BottomRight;
                ScanPitch = m.Recipe.ScanPitch;
                RecipeName = m.Recipe.Name;
                _isEditing = true;
                _oldRecipe = m.Recipe;
                UpdateCalculations();
            });
        });
    }

    // Destructor
    ~ScanSettingsControlViewModel()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private bool CanSaveRecipe()
    {
        return RecipeName.Length > 0 && (_dataReceived || _isEditing);
    }

    private void SaveRecipe()
    {
        if(_isEditing)
        {
            _logger.LogInformation("Updating recipe");
            _recipeManager.UpdateRecipe(_oldRecipe, new ScanRecipe(RecipeName, BottomLeft, TopLeft, TopRight, BottomRight, ScanPitch));
            _oldRecipe = null;
        }
        else
        {
            _logger.LogInformation("Saving recipe");
            _recipeManager.AddRecipe(new ScanRecipe(RecipeName, BottomLeft, TopLeft, TopRight, BottomRight, ScanPitch));
        }
        
        
    }

    // Public methods


    // Private methods
    private void UpdateCalculations()
    {
        OnPropertyChanged("XDimension");
        OnPropertyChanged("YDimension");
        OnPropertyChanged("ScanArea");
        XSampleCount = (int)(XDimension.Value / ScanPitch.Value * 1000);
        YSampleCount = (int)(YDimension.Value / ScanPitch.Value * 1000);
        OnPropertyChanged("TotalSampleCount");
    }
}
