using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DAQ.Enums;
using MeasurementUI.Core.Models;

namespace DAQ.Model
{
    public class ESPDAQ: DAQ
    {
        // Private members
        private readonly ESPDAQController _controller;


        // Public properties
        private bool _initialized;
        public bool Initialized
        {
            get { return _initialized; } 
            set { _initialized = value; }
        }

        // Public methods
        public ESPDAQ()
        {
            _controller = new ESPDAQController();
        }

        public ESPDAQ(SerialConfig serialConfig)
        {
            _controller = new ESPDAQController(serialConfig);
        }

        public async Task<bool> Initialize()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            EventHandler<EventArgs>? InitCompleteEventHandler = null;
            InitCompleteEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
                _controller.InitializationComplete -= InitCompleteEventHandler;
            };
            _controller.InitializationComplete += InitCompleteEventHandler;
            _controller.SendCommand(new Command(OutgoingMessageType.Initialize));
            return await tcs.Task;
        }

        private async Task<bool> SendCommand(Command cmd)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            _controller.RequestComplete += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };
            _controller.SendCommand(cmd);
            return await tcs.Task;
        }

        public float GetVolts()
        {
            return 0.0f;
        }

        public int GetEncoderCounts()
        {
            return 0;
        }

        public void ResetEncoder()
        {
            throw new NotImplementedException();
        }

        // private methods



    }
}
