using MeasurementUI.BusinessLogic.Configuration;
using StageControl.Core.Events;
using StageControl.Interfaces;
using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class SystemController: ISystemController
    {
        #region Private Members
        private readonly MachineConfiguration _machineConfiguration;
        #endregion

        
        public IMachineControl MotionController;

        #region Constructor
        public SystemController(MachineConfiguration machineConfiguration)
        {
            _machineConfiguration = machineConfiguration;
            MotionController = new FNCMachineControl(_machineConfiguration.SerialConfig, _machineConfiguration.StageConfig);
            MotionController.StateChanged += MotionControllerStateChanged;
        }

        private void MotionControllerStateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            MotionControlStatus = FNCMachineControl.ConvertControllerStateToStatus(e.State);
        }
        #endregion

        #region Public Methods
        public async Task Initialize()
        {
            await MotionController.Initialize();
        }
        #endregion

        #region Public Properties

        private string _motionControlStatus; 
        public string MotionControlStatus
        {
            get { return _motionControlStatus; }
            private set { _motionControlStatus = value; }
        }

        #endregion

    }
}
