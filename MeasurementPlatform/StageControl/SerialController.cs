using System.IO.Ports;
using StageControl.Enums;
using StageControl.Consts;

namespace StageControl
{
    public class SerialDataItemReceivedEventArgs: EventArgs
    {
        public SerialDataItem item { get; set; }
    }

    public class SerialController
    {
        private SerialPort port;
        private List<SerialDataItem> messages;
        string currentMessage;

        public event EventHandler<SerialDataItemReceivedEventArgs> SerialDataItemReceived;

        public SerialController()
        {
            messages  = new List<SerialDataItem>();
            currentMessage = "";
            port = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.Open();
        }

        protected virtual void OnSerialDataItemReceived(SerialDataItemReceivedEventArgs e)
        {
            EventHandler<SerialDataItemReceivedEventArgs> handler = SerialDataItemReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseSerialDataItemReceivedEvent(SerialDataItem item)
        {
            SerialDataItemReceivedEventArgs args = new SerialDataItemReceivedEventArgs();
            args.item = item;
            OnSerialDataItemReceived(args);
        }

        private void ParseData()
        {
            bool canContinue = true;
            while(canContinue)
            {
                if (currentMessage.Contains(SerialDataConsts.ESPFirstBootMessageMarker) && currentMessage.Contains(SerialDataConsts.DoubleLineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.DoubleLineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 4), DateTime.Now, SerialDataType.ESPFirstBootMessage);
                    RaiseSerialDataItemReceivedEvent(item);
                    messages.Add(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 4);
                    continue;
                }
                else if (currentMessage.Contains(SerialDataConsts.ESPBootloaderMessageMarker) && currentMessage.Contains(SerialDataConsts.DoubleLineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.DoubleLineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 4), DateTime.Now, SerialDataType.ESPBootloader);
                    RaiseSerialDataItemReceivedEvent(item);
                    messages.Add(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 4);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.MSGINFOMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 2), DateTime.Now, SerialDataType.MSGINFO);
                    RaiseSerialDataItemReceivedEvent(item);
                    messages.Add(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                    continue;
                }
                else if (currentMessage.StartsWith(SerialDataConsts.MSGDBGMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 2), DateTime.Now, SerialDataType.MSGDBG);
                    RaiseSerialDataItemReceivedEvent(item);
                    messages.Add(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                    continue;
                }
                else if(currentMessage.Contains(SerialDataConsts.FNCEntryPromptMessageMarker) && currentMessage.Contains(SerialDataConsts.LineBreak))
                {
                    int endIndex = currentMessage.IndexOf(SerialDataConsts.LineBreak);
                    SerialDataItem item = new SerialDataItem(currentMessage.Substring(0, endIndex + 2), DateTime.Now, SerialDataType.FNCEntryPrompt);
                    RaiseSerialDataItemReceivedEvent(item);
                    messages.Add(item);
                    currentMessage = currentMessage.Remove(0, endIndex + 2);
                    continue;
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