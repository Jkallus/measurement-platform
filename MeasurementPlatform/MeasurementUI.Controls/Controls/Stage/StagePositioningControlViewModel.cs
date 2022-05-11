using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.Controls.Enums;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class StagePositioningControlViewModel : ObservableObject
    {
        private readonly SystemController _systemController;
        private int _realStepSize;


        public IAsyncRelayCommand<HomingAxes> HomeCommand { get; private set; }
        public IAsyncRelayCommand<MotionDirection> MotionCommand { get; private set; }

        private StepSize _stepSize;
        public StepSize StepSize
        {
            get { return _stepSize; }
            set 
            {
                if(SetProperty(ref _stepSize, value))
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
            get {  return _customSize;}
            set { SetProperty(ref _customSize, value); }
        }

        


        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if(SetProperty(ref isBusy, value))
                {
                    HomeCommand.NotifyCanExecuteChanged();
                }
            }
        }




        public StagePositioningControlViewModel(SystemController systemController)
        {
            _systemController = systemController;
            _stepSize = StepSize.TenThousand;
            _customSize = "25000";
            _realStepSize = 10000;
            HomeCommand = new AsyncRelayCommand<HomingAxes>(OnHomeRequested, CanHome);
            MotionCommand = new AsyncRelayCommand<MotionDirection>(OnMotionRequested, CanPerformMotion);
        }

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
                else if(arg == MotionDirection.XNeg)
                    x = -_realStepSize;
                else if(arg == MotionDirection.YPos)
                    y = _realStepSize;
                else if(arg == MotionDirection.YNeg)
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
