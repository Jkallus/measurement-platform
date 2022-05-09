using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;
using MeasurementUI.Core.Models;

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

        // Public properties
        public event EventHandler<ResponseReceivedEventArgs>? ResponseReceived;

        // Public methods
        public void Initialize()
        {
            _port.Open();
            _port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void SendSerialData(string msg)
        {
            _port.Write(msg+"\r");
        }


        // Constructors
        public SerialController(): this(new SerialConfig("COM6"))
        {

        }

        public SerialController(SerialConfig serialConf)
        {
            _currentMessage = "";
            _port = new SerialPort(serialConf.COM, serialConf.BaudRate, serialConf.Parity, serialConf.DataBits, serialConf.StopBits);
            this.Initialize();
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
                            int counts = int.Parse(parts[2]);
                            RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new DataResponse<int>(type, errorCode, counts)));
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
