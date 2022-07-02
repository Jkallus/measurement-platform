using System.IO.Ports;
using StageControl.Enums;
using StageControl.Consts;
using MeasurementUI.Core.Models;
using Microsoft.Extensions.Logging;

namespace StageControl.Model
{
    public class SerialDataItemReceivedEventArgs: EventArgs
    {
        public SerialDataItem? Item { get; set; }

        public SerialDataItemReceivedEventArgs(SerialDataItem item)
        {
            Item = item;
        }
    }

    public class SerialController
    {
        // Private members
        private readonly SerialPort port;
        private string currentMessage;
        private readonly ILogger<SerialController> _logger;

        // Public properties
        public event EventHandler<SerialDataItemReceivedEventArgs>? SerialDataItemReceived;

        // Constructor
        public SerialController(SerialConfig serialConf, ILogger<SerialController> bottomLogger)
        {
            _logger = bottomLogger;
            currentMessage = "";
            port = new SerialPort(serialConf.COM, serialConf.BaudRate, serialConf.Parity, serialConf.DataBits, serialConf.StopBits);
            _logger.LogInformation("Stage SerialController constructed");
        }

        // Public methods
        public void Disconnect()
        {
            port.Close();
        }

        public void Connect()
        {
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.RtsEnable = false;
            port.Open();
            triggerReboot();
        }

        public void SendSerialData(string str)
        {
            port.WriteLine(str);
        }

        public void SendStatusRequest()
        {
            port.Write("?");
        }

        // Private methods
        private void triggerReboot()
        {
            port.RtsEnable = true;
            Thread.Sleep(10);
            port.RtsEnable = false;
        }

        protected virtual void OnSerialDataItemReceived(SerialDataItemReceivedEventArgs e)
        {
            if(SerialDataItemReceived != null)
            {
                EventHandler<SerialDataItemReceivedEventArgs> handler = SerialDataItemReceived;
                handler(this, e);
            }
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            currentMessage += sp.ReadExisting();
            ParseData();
        }

        private void ParseData()
        {
            bool canContinue = true;
            while(canContinue)
            {
                if (currentMessage.StartsWith(SerialDataConsts.ESPFirstBootMessageMarker) && currentMessage.Contains(SerialDataConsts.DoubleLineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.DoubleLineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 4), DateTime.Now, SerialDataType.ESPFirstBootMessage);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex + 4);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.ESPBootloaderMessageMarker) && currentMessage.Contains(SerialDataConsts.DoubleLineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.DoubleLineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 4), DateTime.Now, SerialDataType.ESPBootloader);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex + 4);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.MSGINFOMessageMarker) && currentMessage.Contains(SerialDataConsts.MSGEndMarker))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.MSGEndMarker) + 1;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.MSGINFO);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.MSGDBGMessageMarker) && currentMessage.Contains(SerialDataConsts.MSGEndMarker))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.MSGEndMarker) + 1;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.MSGDBG);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex);
                    continue;
                }
                else if(currentMessage.StartsWith(SerialDataConsts.FNCEntryPromptMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 2), DateTime.Now, SerialDataType.FNCEntryPrompt);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                    continue;
                }
                else if(currentMessage.StartsWith(SerialDataConsts.StatusStartMessageMarker) && currentMessage.Contains(SerialDataConsts.StatusEndMessageMarker))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.StatusEndMessageMarker) + 1;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.Status);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex);
                }
                else if(currentMessage.StartsWith(SerialDataConsts.RequestCompleteMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.RequestComplete);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                }
                else if(currentMessage.StartsWith(SerialDataConsts.RuntimeErrorMessageMarker) && currentMessage.Contains(SerialDataConsts.Newline))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.Newline);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.RuntimeError);
                    OnSerialDataItemReceived(new SerialDataItemReceivedEventArgs(item));
                    currentMessage = currentMessage.Remove(0, endIndex + 1);
                }
                else if (currentMessage.StartsWith(SerialDataConsts.LineBreak))
                {
                    currentMessage = currentMessage.Remove(0, 2);
                }
                else // was not able to match any marker so exit look and wait for more serial data to come in
                {
                    canContinue = false;
                }   
            }
        }
    }
}