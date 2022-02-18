using System.IO.Ports;
using StageControl.Enums;
using StageControl.Consts;

namespace StageControl.Model
{
    public class SerialDataItemReceivedEventArgs: EventArgs
    {
        public SerialDataItem? Item { get; set; }

        public SerialDataItemReceivedEventArgs(SerialDataItem item)
        {
            Item = item;
        }

        public SerialDataItemReceivedEventArgs()
        {

        }

    }

    public class SerialController
    {
        private readonly SerialPort port;
        string currentMessage;

        public event EventHandler<SerialDataItemReceivedEventArgs>? SerialDataItemReceived;

        public SerialController()
        {
            currentMessage = "";
            port = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        }


        public SerialController(SerialConfig serialConf)
        {
            currentMessage = "";
            port = new SerialPort(serialConf.COM, serialConf.BaudRate, serialConf.Parity, serialConf.DataBits, serialConf.StopBits);
        }

        private void triggerReboot()
        {
            port.RtsEnable = false;
            port.DtrEnable = true;
            Thread.Sleep(10);
            port.RtsEnable = true;
            port.DtrEnable = false;
            Thread.Sleep(10);
            port.RtsEnable = true;
            port.DtrEnable = true;
            Thread.Sleep(10);
        }

        public void Connect()
        {
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.DtrEnable = false;
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
        
        protected virtual void OnSerialDataItemReceived(SerialDataItemReceivedEventArgs e)
        {
            if(SerialDataItemReceived != null)
            {
                EventHandler<SerialDataItemReceivedEventArgs> handler = SerialDataItemReceived;
                handler(this, e);
            }
        }

        private void RaiseSerialDataItemReceivedEvent(SerialDataItem item)
        {
            SerialDataItemReceivedEventArgs args = new SerialDataItemReceivedEventArgs(item);
            OnSerialDataItemReceived(args);
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
                    RaiseSerialDataItemReceivedEvent(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 4);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.ESPBootloaderMessageMarker) && currentMessage.Contains(SerialDataConsts.DoubleLineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.DoubleLineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 4), DateTime.Now, SerialDataType.ESPBootloader);
                    RaiseSerialDataItemReceivedEvent(item);
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
                    RaiseSerialDataItemReceivedEvent(item);
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
                    RaiseSerialDataItemReceivedEvent(item);
                    currentMessage = currentMessage.Remove(0, endIndex);
                    continue;
                }
                else if(currentMessage.StartsWith(SerialDataConsts.FNCEntryPromptMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 2), DateTime.Now, SerialDataType.FNCEntryPrompt);
                    RaiseSerialDataItemReceivedEvent(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                    continue;
                }
                else if(currentMessage.StartsWith(SerialDataConsts.StatusStartMessageMarker) && currentMessage.Contains(SerialDataConsts.StatusEndMessageMarker))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.StatusEndMessageMarker) + 1;
                    if (currentMessage.Substring(endIndex).StartsWith(SerialDataConsts.LineBreak))
                        endIndex += 2;
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.Status);
                    RaiseSerialDataItemReceivedEvent(item);
                    currentMessage = currentMessage.Remove(0, endIndex);
                }
                else if(currentMessage.StartsWith(SerialDataConsts.RequestCompleteMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex), DateTime.Now, SerialDataType.RequestComplete);
                    RaiseSerialDataItemReceivedEvent(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
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

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            currentMessage += sp.ReadExisting();
            ParseData();
        }
    }
}