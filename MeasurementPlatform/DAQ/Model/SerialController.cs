using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementUI.Core.Models;

namespace DAQ.Model
{
    public class SerialController
    {
        // Private member variables
        private readonly SerialPort _port;
        private string _currentMessage;

        // Public properties
        public event EventHandler<EventArgs>? MessageReceived;

        // Public methods
        public void Initialize()
        {
            _port.Open();
            _port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void SendSerialData(string msg)
        {
            _port.WriteLine(msg);
        }


        // Constructors
        public SerialController(): this(new SerialConfig("COM6"))
        {

        }

        public SerialController(SerialConfig serialConf)
        {
            _currentMessage = "";
            _port = new SerialPort(serialConf.COM, serialConf.BaudRate, serialConf.Parity, serialConf.DataBits, serialConf.StopBits);
        }

        // Private methods
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            _currentMessage += sp.ReadExisting();
            ParseData();
        }

        private void ParseData()
        {
            if(_currentMessage.Contains("\n\r"))
            {
                int endIndex = _currentMessage.IndexOf("/n/r") + 2;
                string message = _currentMessage.Substring(0, endIndex);
                _currentMessage.Remove(0, endIndex);
            }
        }
    }
}
