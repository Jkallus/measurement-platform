using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;
using MeasurementUI.Core.Models;
using Microsoft.Extensions.Logging;

namespace DAQ.Model
{
    /// <summary>
    ///  Class <c>SerialController</c> Responsible to handling serial interface layer. Writes data to serialport and receives responses. 
    ///  Primary functionality is identifying when a whole response arrives over serial and captures that to propagate to the next layer
    /// </summary>
    public class SerialController
    {
        // Private member variables
        private readonly SerialPort _port;
        private string _currentMessage;
        private readonly ILogger<SerialController> _logger;
        // Public properties
        public event EventHandler<ResponseReceivedEventArgs>? ResponseReceived;


        public bool IsSerialportInitialized
        {
            get { return _port.IsOpen; }
        }

        // Public methods
        public void Initialize()
        {
            try
            {
                _port.Open();
                _port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
            catch(System.UnauthorizedAccessException ex)
            {
                _logger.LogError(ex.Message);
            }
            catch(System.IO.FileNotFoundException ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
            
        }

        public void SendSerialData(string msg)
        {
            _port.Write(msg+"\r");
        }


        // Constructors
        //public SerialController(): this(new SerialConfig("COM6"))
        //{

        //}

        public SerialController(SerialConfig serialConf, ILogger<SerialController> bottomLogger)
        {
            _logger = bottomLogger;
            _currentMessage = "";
            _port = new SerialPort(serialConf.COM, serialConf.BaudRate, serialConf.Parity, serialConf.DataBits, serialConf.StopBits);
            //this.Initialize();
        }

        

        // Private methods
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            _currentMessage += sp.ReadExisting();
            ParseData();
        }

        protected virtual void RaiseResponseReceivedEvent(ResponseReceivedEventArgs e)
        {
            if(ResponseReceived != null)
            {
                EventHandler<ResponseReceivedEventArgs> handler = ResponseReceived;
                handler(this, e);
            }
        }

        private void ParseData()
        {

            if(_currentMessage.Contains("\n\r"))
            {
                int endIndex = _currentMessage.IndexOf("\n\r"); // grab complete message not including \n\r
                string message = _currentMessage.Substring(0, endIndex); // separate this message from any potential others coming next
                _currentMessage = _currentMessage.Remove(0, endIndex + 2); // this message is sure to be processed so safe to remove from buffer, remove \n\r here
                Response ret;

                string[] parts = message.Split(';', StringSplitOptions.RemoveEmptyEntries);
                MessageType type = (MessageType)Enum.Parse(typeof(MessageType), parts[0]);
                ErrorCode errorCode = (ErrorCode)Enum.Parse(typeof(ErrorCode), parts[1]);
                if(parts.Length > 2)
                {
                    switch (type)
                    {
                        case MessageType.GetVoltage:
                            float voltage = float.Parse(parts[2]);
                            RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new DataResponse<float>(type, errorCode, voltage)));
                            break;
                        case MessageType.GetEncoderCounts:
                            string[] counts = parts[2].Split(',', StringSplitOptions.TrimEntries);
                            var data = Tuple.Create<int, int>(int.Parse(counts[0]), int.Parse(counts[1]));
                            RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new DataResponse<Tuple<int, int>>(type, errorCode, data)));
                            break;
                    }
                }
                else
                {
                    RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new Response(type, errorCode)));
                }

                

            }
        }
    }
}
