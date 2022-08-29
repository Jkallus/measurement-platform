using MeasurementApp.Controls.Enums;
using MeasurementApp.Services;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using StageControl.Enums;
using StageControl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls;

public class StagePositioningControlViewModel: ObservableRecipient
{
    // Private member variables
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemController _systemController;
    private int _realStepSize;

    // Public properties
    public IAsyncRelayCommand GoToTargetCommand { get; private set; }
    public IAsyncRelayCommand<HomingAxes> HomeCommand { get; private set; }
    public IAsyncRelayCommand<MotionDirection> MotionCommand { get; private set; }

    private StepSize _stepSize;
    public StepSize StepSize
    {
        get => _stepSize;
        set
        {
            if (SetProperty(ref _stepSize, value))
            {
                switch (StepSize)
                {
                    case StepSize.FiftyThousand:
                        _realStepSize = 50000;
                        break;
                    case StepSize.TenThousand:
                        _realStepSize = 10000;
                        break;
                    case StepSize.OneThousand:
                        _realStepSize = 1000;
                        break;
                    case StepSize.Custom:
                        _realStepSize = int.Parse(CustomSize);
                        break;
                    default:
                        _realStepSize = 10000;
                        break;
                }

            }
        }
    }

    private string _customSize;
    public string CustomSize
    {
        get => _customSize;
        set => SetProperty(ref _customSize, value);
    }

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (SetProperty(ref isBusy, value))
            {
                HomeCommand.NotifyCanExecuteChanged();
                MotionCommand.NotifyCanExecuteChanged();
                GoToTargetCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private (float XCoordinate, float YCoordinate) _targetPosition;
    public string TargetPosition => $"{_targetPosition.XCoordinate.ToString("0.000")}, {_targetPosition.YCoordinate.ToString("0.000")}";

    private int _stepSizeButtonIndex;
    public int StepSizeButtonIndex
    {
        get => _stepSizeButtonIndex;
        set
        {
            if(SetProperty(ref _stepSizeButtonIndex, value))
            {
                switch(StepSizeButtonIndex)
                {
                    case 0:
                        StepSize = StepSize.FiftyThousand;
                        break;
                    case 1:
                        StepSize = StepSize.TenThousand;
                        break;
                    case 2:
                        StepSize = StepSize.OneThousand;
                        break;
                    case 3:
                        StepSize = StepSize.Custom;
                        break;
                }    
            }
        }
    }

    // Constructor
    public StagePositioningControlViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _systemController = _serviceProvider.GetService(typeof(SystemController)) as SystemController ?? throw new Exception("SystemController is null");
        _stepSize = StepSize.TenThousand;
        _customSize = "25000";
        _realStepSize = 10000;
        _stepSizeButtonIndex = 1;
        HomeCommand = new AsyncRelayCommand<HomingAxes>(OnHomeRequested, CanHome);
        MotionCommand = new AsyncRelayCommand<MotionDirection>(OnMotionRequested, CanPerformMotion);
        GoToTargetCommand = new AsyncRelayCommand(OnGoToTargetRequested, CanGoToTarget);
        _systemController.MotionController.RuntimeError += MotionController_RuntimeError;
        _systemController.MotionController.StateChanged += MotionController_StateChanged;
        WeakReferenceMessenger.Default.Register<StageTargetPositionChangedMessage>(this, (r, m) =>
        {
            App.MainRoot!.DispatcherQueue.TryEnqueue(() =>
            {
                _targetPosition = m.TargetLocation;
                OnPropertyChanged("TargetPosition");
            });
        });
    }

    private async Task OnGoToTargetRequested()
    {
        try
        {
            IsBusy = true;
            //await _systemController.Jog( (int)(_targetPosition.XCoordinate * 1000), (int)(_targetPosition.YCoordinate * 1000), JogType.Absolute, this);
            await _systemController.MoveTo((_targetPosition.XCoordinate, _targetPosition.YCoordinate), StageControl.Model.BlockingType.NonBlocking, this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGoToTarget()
    {
        if (!_systemController.IsMotionControllerConnected)
            return false;
        else if (IsBusy)
            return false;
        else if (_systemController.MotionController.IsHomed == false)
            return false;
        else return true;
    }

    private void MotionController_StateChanged(object? sender, FNCStateChangedEventArgs e)
    {
        MotionCommand.NotifyCanExecuteChanged();
        HomeCommand.NotifyCanExecuteChanged();
        GoToTargetCommand.NotifyCanExecuteChanged();
    }

    private void MotionController_RuntimeError(object? sender, RuntimeErrorEventArgs e)
    {
        App.MainRoot!.DispatcherQueue.TryEnqueue(async () =>
        {
            await App.MainRoot.MessageDialogAsync("Motion Error", e.Message);
        });
    }

    // Private Methods
    private bool CanPerformMotion(MotionDirection obj)
    {
        if (!_systemController.IsMotionControllerConnected)
            return false;
        else if (IsBusy)
            return false;
        else if (_systemController.MotionController.IsHomed == false)
            return false;
        else return true;
    }

    private async Task OnMotionRequested(MotionDirection arg)
    {
        try
        {
            int x = 0;
            int y = 0;
            if (arg == MotionDirection.XPos)
                x = _realStepSize;
            else if (arg == MotionDirection.XNeg)
                x = -_realStepSize;
            else if (arg == MotionDirection.YPos)
                y = _realStepSize;
            else if (arg == MotionDirection.YNeg)
                y = -_realStepSize;

            IsBusy = true;
            await _systemController.Jog(x, y, JogType.Incremental, this);
        }
        finally
        {
            IsBusy = false;
            //MotionCommand.NotifyCanExecuteChanged();
            //HomeCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task OnHomeRequested(HomingAxes axes)
    {
        try
        {
            IsBusy = true;
            await _systemController.Home(axes, this);
        }
        finally
        {
            IsBusy = false;
            //MotionCommand.NotifyCanExecuteChanged();
        }

    }

    private bool CanHome(HomingAxes axes)
    {
        return !IsBusy && _systemController.IsMotionControllerConnected;
    }

}