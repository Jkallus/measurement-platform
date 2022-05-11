using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DAQ.Enums;
using DAQ.Interfaces;
using MeasurementUI.Core.Models;

namespace DAQ.Model
{
    public class ESPDAQ: IDAQ
    {
        // Private members
        private readonly ESPDAQController _controller;


        // Public properties
        private bool _initialized;

        public event EventHandler<DAQStateEventArgs>? StateChanged
        {
            add => this._controller.StateChanged += value;
            remove => this._controller.StateChanged -= value;
        }

        public bool Initialized
        {
            get { return _controller.IsInitialized; } 
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

        public async Task Initialize()
        {
            await SendCommand(new Command(MessageType.Initialize));
        }

        public async Task Deinitialize()
        {
            await SendCommand(new Command(MessageType.Deinitialize));
        }

        public async Task<float> GetVolts()
        {
            return await SendDataCommand<float>(new Command(MessageType.GetVoltage));
        }

        public async Task<int> GetEncoderCounts()
        {
            return await SendDataCommand<int>(new Command(MessageType.GetEncoderCounts));
        }

        public async Task ResetEncoder()
        {
            await SendCommand(new Command(MessageType.ResetEncoder));
        }

        


        // private methods
        private async Task SendCommand(Command cmd)
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            EventHandler<ResponseReceivedEventArgs>? CommandCompleteEventHandler = null;
            CommandCompleteEventHandler = (sender, e) =>
            {
                _controller.CommandComplete -= CommandCompleteEventHandler;
                if (e.Response.ErrorCode == ErrorCode.Success)
                {
                    tcs.SetResult();
                }
                else
                {
                    tcs.SetException(new DAQException(e.Response.ErrorCode));
                }
            };
            _controller.CommandComplete += CommandCompleteEventHandler;
            _controller.SendCommand(cmd);
            await tcs.Task;
        }

        private async Task<T> SendDataCommand<T>(Command cmd)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            EventHandler<ResponseReceivedEventArgs>? CommandCompleteEventHandler = null;
            CommandCompleteEventHandler = (sender, e) =>
            {
                _controller.CommandComplete -= CommandCompleteEventHandler;
                if (e.Response.ErrorCode == ErrorCode.Success)
                {
                    DataResponse<T>? result = e.Response as DataResponse<T>;
                    tcs.SetResult(result!.Data);
                }
                else
                {
                    tcs.SetException(new DAQException(e.Response.ErrorCode));
                }
            };
            _controller.CommandComplete += CommandCompleteEventHandler;
            _controller.SendCommand(cmd);
            return await tcs.Task;
        }
    }
}
