using MeasurementApp.Controls.Enums;
using MeasurementApp.Services;
using MeasurementUI.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using StageControl.Enums;
using StageControl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls
{
    public class StagePositioningControlViewModel: ObservableObject
    {
        // Private member variables
        private readonly IServiceProvider _serviceProvider;
        private readonly SystemController _systemController;
        private int _realStepSize;

        // Public properties
        public IAsyncRelayCommand<HomingAxes> HomeCommand { get; private set; }
        public IAsyncRelayCommand<MotionDirection> MotionCommand { get; private set; }

        private StepSize _stepSize;
        public StepSize StepSize
        {
            get { return _stepSize; }
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
            get { return _customSize; }
            set { SetProperty(ref _customSize, value); }
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
                }
            }
        }

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
            _systemController = _serviceProvider.GetService(typeof(SystemController)) as SystemController;
            _stepSize = StepSize.TenThousand;
            _customSize = "25000";
            _realStepSize = 10000;
            HomeCommand = new AsyncRelayCommand<HomingAxes>(OnHomeRequested, CanHome);
            MotionCommand = new AsyncRelayCommand<MotionDirection>(OnMotionRequested, CanPerformMotion);
            _systemController.MotionController.RuntimeError += MotionController_RuntimeError;
        }

        private void MotionController_RuntimeError(object sender, RuntimeErrorEventArgs e)
        {
            App.MainRoot.DispatcherQueue.TryEnqueue(async () =>
            {
                await App.MainRoot.MessageDialogAsync("Motion Error", e.Message);
            });
        }

        // Private Methods
        private bool CanPerformMotion(MotionDirection obj)
        {
            if (IsBusy)
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
                await _systemController.Jog(x, y, JogType.Incremental);
            }
            finally
            {
                IsBusy = false;
                MotionCommand.NotifyCanExecuteChanged();
                HomeCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task OnHomeRequested(HomingAxes axes)
        {
            try
            {
                IsBusy = true;
                await _systemController.Home(axes);
            }
            finally
            {
                IsBusy = false;
                MotionCommand.NotifyCanExecuteChanged();
            }

        }

        private bool CanHome(HomingAxes axes)
        {
            return !IsBusy;
        }

    }
}