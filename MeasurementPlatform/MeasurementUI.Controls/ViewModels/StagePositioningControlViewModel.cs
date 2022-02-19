using MeasurementUI.BusinessLogic.SystemControl;
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
    public class StagePositioningControlViewModel: ObservableObject
    {
        private readonly SystemController _systemController;

        public IAsyncRelayCommand<HomingAxes> HomeCommand { get; private set; }

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
            HomeCommand = new AsyncRelayCommand<HomingAxes>(OnHomeRequested, CanHome);
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
            }
            
        }

        private bool CanHome(HomingAxes axes)
        {
            return !IsBusy;
        }
        
    }
}
