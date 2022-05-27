using DAQ.Interfaces;
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
using DAQ.Model;
using DAQ.Enums;


namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class SystemController: ObservableObject, ISystemController
    {
        #region Private Members
        private readonly IServiceProvider _serviceProvider;
        private readonly MachineConfiguration _machineConfiguration;
        
        #endregion

        
        public readonly IMachineControl MotionController;
        public readonly IDAQ DAQ;

        #region Constructor
        public SystemController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _machineConfiguration = _serviceProvider.GetService(typeof(MachineConfiguration)) as MachineConfiguration;
            _motionControllerStatus = "";
            MotionController = new FNCMachineControl(_machineConfiguration.StageSerialConfig, _machineConfiguration.StageConfig);
            MotionController.StateChanged += MotionController_StateChanged;
            MotionController.UnexpectedRestart += MotionController_UnexpectedRestart;

            DAQ = new ESPDAQ(_machineConfiguration.DAQSerialConfig);
            DAQ.StateChanged += DAQ_StateChanged;
            
        }

        private void MotionController_UnexpectedRestart(object? sender, EventArgs e)
        {
            
        }

        private void MotionController_StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            MotionControllerStatus = FNCMachineControl.ConvertControllerStateToStatus(e.State);
        }

        private void DAQ_StateChanged(object? sender, DAQStateEventArgs e)
        {
            DAQStatus = e.State;
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
            try
            {
                var task1 = MotionController.Initialize();
                var task2 = DAQ.Initialize();
                await Task.WhenAll(task1, task2);
            }
            catch (DAQException ex)
            {
                if (ex.DAQError != ErrorCode.AlreadyInitialized)
                {
                    throw ex;
                }
            }
            
        }

        public async Task Deinitialize()
        {
            MotionController.Deinitialize();
            await DAQ.Deinitialize();
        }
        #endregion

        #region Public Properties

        private string _motionControllerStatus; 
        public string MotionControllerStatus
        {
            get { return _motionControllerStatus; }
            private set { SetProperty(ref _motionControllerStatus, value); }
        }

        private string _daqStatus;
        public string DAQStatus
        {
            get { return _daqStatus; }
            private set { SetProperty(ref _daqStatus, value); }
        }

        public bool IsMotionControllerConnected
        {
            get { return MotionController.IsConnected; }
        }

        #endregion

    }
}
