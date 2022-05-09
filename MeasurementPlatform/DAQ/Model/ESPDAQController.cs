using MeasurementUI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;

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
        private Command? _currentCommand;

        // Public properties
        //public event EventHandler<EventArgs>? InitializationComplete;
        public event EventHandler<ResponseReceivedEventArgs>? CommandComplete;
        public bool IsInitialized { get; set; }

        // Constructors
        public ESPDAQController()
        {
            _serial = new SerialController();
            _serial.ResponseReceived += _serial_ResponseReceived;
        }

        private void _serial_ResponseReceived(object? sender, ResponseReceivedEventArgs e)
        {
            if (e.Response != null)
            {
                if(_currentCommand != null)
                {
                    if(_currentCommand.MessageType == e.Response.MessageType)
                    {
                        OnCommandComplete(e);
                    }
                    else
                    {
                        // Raise unexpected message
                    }
                }
            }
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
        protected virtual void OnCommandComplete(ResponseReceivedEventArgs e)
        {
            if(CommandComplete != null)
            {
                EventHandler<ResponseReceivedEventArgs> handler = CommandComplete;
                handler(this, e);
            }
        }

        //protected virtual void OnInitializationComplete(ResponseReceivedEventArgs e)
        //{
        //    if(InitializationComplete != null)
        //    {
        //        EventHandler<EventArgs> handler = InitializationComplete;
        //        handler(this, e);
        //    }
        //}

    }
}
