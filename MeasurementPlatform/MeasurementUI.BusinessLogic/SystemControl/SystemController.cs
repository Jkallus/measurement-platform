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
using MeasurementUI.BusinessLogic.SystemControl.Enums;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class SystemController: ObservableObject, ISystemController
    {
        #region Private Members
        private readonly IServiceProvider _serviceProvider;
        private readonly MachineConfiguration _machineConfiguration;
        private readonly ILogger<SystemController> _systemLogger;
        
        #endregion

        
        public readonly IMachineControl MotionController;
        public readonly IDAQ DAQ;

        #region Constructor
        public SystemController(IServiceProvider serviceProvider, ILogger<SystemController> logger)
        {
            _serviceProvider = serviceProvider;
            _systemLogger = logger;
            _systemLogger.LogInformation("System Controller object constructed");
            _machineConfiguration = _serviceProvider.GetService(typeof(MachineConfiguration)) as MachineConfiguration;
            _motionControllerStatus = "";
            //MotionController = new FNCMachineControl(_machineConfiguration.StageSerialConfig, _machineConfiguration.StageConfig, Log.ForContext<FNCMachineControl>());
            MotionController = _serviceProvider.GetService(typeof(IMachineControl)) as IMachineControl;
            MotionController.StateChanged += MotionController_StateChanged;
            MotionController.UnexpectedRestart += MotionController_UnexpectedRestart;

            //DAQ = new ESPDAQ(_machineConfiguration.DAQSerialConfig);
            DAQ = _serviceProvider.GetService(typeof(IDAQ)) as IDAQ;
            DAQ.StateChanged += DAQ_StateChanged;
           
        }

        private void MotionController_UnexpectedRestart(object? sender, EventArgs e)
        {
            
        }

        private void MotionController_StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            MotionControllerStatus = FNCMachineControl.ConvertControllerStateToStatus(e.State);
            if(e.State == LifetimeFNCState.Unknown)
            {
                MotionControllerInitializationState = ModuleInitializationState.Uninitialized;
            }
            else if(e.State == LifetimeFNCState.FNCReady)
            {
                MotionControllerInitializationState = ModuleInitializationState.Initialized;
            }
            else
            {
                MotionControllerInitializationState = ModuleInitializationState.Initializing;
            }
        }

        private void DAQ_StateChanged(object? sender, DAQStateEventArgs e)
        {
            DAQStatus = e.ToFriendlyString();
            switch(e.State)
            {
                case DAQState.Uninitialized:
                    DAQInitializationState = ModuleInitializationState.Uninitialized;
                    break;
                case DAQState.Initializing:
                    DAQInitializationState = ModuleInitializationState.Initializing;
                    break;
                case DAQState.Initialized:
                    DAQInitializationState = ModuleInitializationState.Initialized;
                    break;
            }
        }

        private void MotionController_PositionChanged(object? sender, PositionChangedEventArgs e)
        {
            
        }
        #endregion

        #region Public Methods
        public async Task Home(HomingAxes axes)
        {
            _systemLogger.LogInformation("Homing {Axis}", axes.ToString());
            await MotionController.Home(axes);
        }

        public async Task Jog(int x, int y, JogType type)
        {
            _systemLogger.LogInformation("Jogging {Xcoords},{Ycoords},{JogType}", x, y, type.ToString());
            await MotionController.Jog(x, y, type);
        }


        public async Task Initialize()
        {
            try
            {
                _systemLogger.LogInformation("Initializing system");
                var task1 = MotionController.Initialize();
                var task2 = DAQ.Initialize();
                await Task.WhenAll(task1, task2);
            }
            catch (DAQException ex)
            {
                if (ex.DAQError != ErrorCode.AlreadyInitialized)
                {
                    _systemLogger.LogError(ex, "Caught DAQError");
                    throw ex;
                }
            }
            
        }

        public async Task Deinitialize()
        {
            _systemLogger.LogInformation("Deinitializing system");
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

        private ModuleInitializationState _daqInitializationState;
        public ModuleInitializationState DAQInitializationState
        {
            get { return _daqInitializationState; }
            set { SetProperty(ref _daqInitializationState, value); }
        }

        private ModuleInitializationState _motionControllerInitializationState;
        public ModuleInitializationState MotionControllerInitializationState
        {
            get { return _motionControllerInitializationState; }
            set { SetProperty(ref _motionControllerInitializationState, value); }
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
