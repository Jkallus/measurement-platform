using MeasurementUI.BusinessLogic.Configuration;
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

        private readonly MachineConfiguration _machineConfiguration;

        public IMachineControl MotionController;

        public SystemController(MachineConfiguration machineConfiguration)
        {
            _machineConfiguration = machineConfiguration;
            MotionController = new FNCMachineControl(_machineConfiguration.SerialConfig, _machineConfiguration.StageConfig);
        }

        public void Initialize()
        {
            MotionController.Initialize();
        }
    }
}
