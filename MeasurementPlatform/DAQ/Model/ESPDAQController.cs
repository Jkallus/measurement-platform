using MeasurementUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;
using Microsoft.Extensions.Logging;

namespace DAQ.Model
{
    /// <summary>
    /// Class <c>ESPDAQController</c> is the middle layer responsible for taking in complete serial messages and interpreting them in the context of commands issued.
    /// The ESPDAQController is also responsible for constructing serial messages from commands and sending them with the SerialController.
    /// 
    /// Order of events when a request is made:
    ///     1. The event handler for RequestComplete has a new event handling method added to it. 
    ///     2. A command is sent by ESPDAQController.SendCommand()
    ///     3. A message is recieved and reconstructed by the SerialController
    ///     4. When the message is complete it is forwarded to the ResponseReceived handling method in ESPDAQController
    ///     5. Its determined if the message is a response to the recent command or an unprompted runtime message
    ///     6. If its a response to the recent command then the RequestComplete handler is called
    ///     7. The handler reads the response values and if theres an error it sets an exception for the caller
    /// </summary>
    public class ESPDAQController
    {
        // Private members
        private readonly SerialController _serial;
        private Command? _currentCommand; // pending command
        private bool _initCommandPending; // are we waiting for a response to an init or deinit command
        private readonly ILogger<ESPDAQController> _logger;

        // Public properties
        //public event EventHandler<EventArgs>? InitializationComplete;
        public event EventHandler<ResponseReceivedEventArgs>? CommandComplete;
        public event EventHandler<DAQStateEventArgs>? StateChanged;

        private bool _isInitialized;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                _isInitialized = value;
                OnStateChanged(new DAQStateEventArgs(_isInitialized ? DAQState.Initialized : DAQState.Uninitialized));
            }
        }

        


        // Constructors
        //public ESPDAQController(): this(new SerialConfig("COM6"))
        //{
        //}

        public ESPDAQController(SerialConfig serialConfig, ILogger<ESPDAQController> middleLogger, ILogger<SerialController> bottomLogger)
        {
            _logger = middleLogger;
            _serial = new SerialController(serialConfig, bottomLogger);
            _serial.ResponseReceived += _serial_ResponseReceived;
            _initCommandPending = false;
            IsInitialized = false;
        }

        private void _serial_ResponseReceived(object? sender, ResponseReceivedEventArgs e)
        {
            if (e.Response != null)
            {
                if(_currentCommand != null && _currentCommand.MessageType == e.Response.MessageType)
                {
                    if (_initCommandPending)
                        CheckInitialized(e);
                        _initCommandPending = false;

                    OnCommandComplete(e);
                }
                else // either theres no current command or the response type doesnt match the command type, either way this is unexpected feedback
                {
                    // TODO handle unexpected feedback
                }
            }
        }

        

        // Public methods
        public void SendCommand(Command cmd)
        {
            if(!_serial.IsSerialportInitialized)
            {
                _serial.Initialize();
            }
            if (cmd.MessageType == MessageType.Initialize || cmd.MessageType == MessageType.Deinitialize) // mark init command pending if the request was either init or deinit
                _initCommandPending = true;
            if(cmd.MessageType == MessageType.Initialize)
                OnStateChanged(new DAQStateEventArgs(DAQState.Initializing));
            _serial.SendSerialData(cmd.ToString());
            _currentCommand = cmd;
        }

        // Private methods
        protected virtual void OnCommandComplete(ResponseReceivedEventArgs e)
        {
            if(CommandComplete != null)
            {
                EventHandler<ResponseReceivedEventArgs> handler = CommandComplete;
                handler(this, e);
            }
        }

        protected virtual void OnStateChanged(DAQStateEventArgs e)
        {
            if(StateChanged != null)
            {
                EventHandler<DAQStateEventArgs> handler = StateChanged;
                handler(this, e);
            }
        }

        private void CheckInitialized(ResponseReceivedEventArgs e)
        {
            if(e.Response.MessageType == MessageType.Initialize) // responding to an init command
            {
                if(e.Response.ErrorCode == ErrorCode.Success || e.Response.ErrorCode == ErrorCode.AlreadyInitialized)
                    IsInitialized = true; // in these cases we are initialized
                else
                    IsInitialized = false; // any other error codes for init mean DAQ is not initialized
            }
            if(e.Response.MessageType == MessageType.Deinitialize)
            {
                IsInitialized = false; // regardless of success it means DAQ is deinitialized
            }

        }




    }
}
