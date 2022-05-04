using MeasurementUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Model
{
    public class ESPDAQController
    {
        // Private members
        private readonly SerialController _serial;
        private Command? _currentCommand;

        // Public properties
        public event EventHandler<EventArgs>? InitializationComplete;

        public event EventHandler<RequestCompleteEventArgs>? RequestComplete;

        // Constructors
        public ESPDAQController()
        {
            _serial = new SerialController();
        }

        public ESPDAQController(SerialConfig serialConfig)
        {
            _serial = new SerialController(serialConfig);
        }

        // Public methods
        public void SendCommand(Command cmd)
        {
            _serial.SendSerialData(cmd.ToString());
            _currentCommand = cmd;
        }

        // Private methods


    }
}
