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

        public StagePositioningControlViewModel(SystemController systemController)
        {
            _systemController = systemController;
            HomeCommand = new AsyncRelayCommand<HomingAxes>(OnHomeRequested, CanHome);
        }

        private async Task OnHomeRequested(HomingAxes axes)
        {
            await _systemController.Home(axes);
        }

        private bool CanHome(HomingAxes axes)
        {
            return true;
        }
        
    }
}
