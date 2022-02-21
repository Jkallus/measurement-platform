using MeasurementUI.BusinessLogic.Configuration;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StageControl.Enums;
using StageControl.Events;
using StageControl.Interfaces;
using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class SystemController: ObservableObject, ISystemController
    {
        #region Private Members
        private readonly MachineConfiguration _machineConfiguration;
        #endregion

        
        public readonly IMachineControl MotionController;

        #region Constructor
        public SystemController(MachineConfiguration machineConfiguration)
        {
            _machineConfiguration = machineConfiguration;
            _motionControllerStatus = "";
            MotionController = new FNCMachineControl(_machineConfiguration.SerialConfig, _machineConfiguration.StageConfig);
            MotionController.StateChanged += MotionController_StateChanged;
            MotionController.UnexpectedRestart += MotionController_UnexpectedRestart;
            
        }

        private void MotionController_UnexpectedRestart(object? sender, EventArgs e)
        {
            
        }

        private void MotionController_StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            MotionControllerStatus = FNCMachineControl.ConvertControllerStateToStatus(e.State);
        }

        private void MotionController_PositionChanged(object? sender, PositionChangedEventArgs e)
        {
            
        }
        #endregion

        #region Public Methods
        public async Task Home(HomingAxes axes)
        {
            await MotionController.Home(axes);
        }

        public async Task Jog(int x, int y, JogType type)
        {
            await MotionController.Jog(x, y, type);
        }


        public async Task Initialize()
        {
            await MotionController.Initialize();
        }

        public void Deinitialize()
        {
            MotionController.Deinitialize();
        }
        #endregion

        #region Public Properties

        private string _motionControllerStatus; 
        public string MotionControllerStatus
        {
            get { return _motionControllerStatus; }
            private set { SetProperty(ref _motionControllerStatus, value); }
        }

        public bool IsMotionControllerConnected
        {
            get { return MotionController.IsConnected; }
        }

        #endregion

    }
}
