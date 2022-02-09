using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl
{
    public class FNCStateChangedEventArgs : EventArgs 
    {
        public LifetimeFNCState State { get; set; }

        public FNCStateChangedEventArgs(LifetimeFNCState state)
        {
            this.State = state;
        }

        public FNCStateChangedEventArgs()
        {

        }
    }


    public class FluidNCController
    {
        private readonly SerialController serial;
        private readonly List<SerialDataItem> messages;

        public event EventHandler <FNCStateChangedEventArgs>? FNCStateChanged;
        
        private LifetimeFNCState state;
        public LifetimeFNCState State
        {
            get { return state; }
            private set { state = value; }  
        }

        public FluidNCController()
        {
            serial = new SerialController();
            messages = new List<SerialDataItem>();
            state = LifetimeFNCState.Unknown;
        }

        public void RequestStatus()
        {
            serial.SendSerialData("?");
        }

        private void DataReceived(object? sender, SerialDataItemReceivedEventArgs e)
        {
            if(e.Item != null)
            {
                ProcessIncomingSerialDataItem(e.Item);
                Console.WriteLine(e.Item.ToString());
            }
        }

        protected virtual void OnFNCStateChanged(FNCStateChangedEventArgs e)
        {
            if(FNCStateChanged != null)
            {
                EventHandler<FNCStateChangedEventArgs> handler = FNCStateChanged;
                handler(this, e);
            }            
        }

        private void ProcessIncomingSerialDataItem(SerialDataItem item)
        {
            if(state == LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // expected transition when starting up
            {
                state = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            else if(state != LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // if we get this at any other point that means a reset occured and FNC state is lost
            {
                state = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            else if(state == LifetimeFNCState.FirstBoot && item.Type == SerialDataType.ESPBootloader) // next state in startup routine
            {
                state = LifetimeFNCState.SecondBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            else if(state == LifetimeFNCState.SecondBoot && item.Type == SerialDataType.MSGINFO) // first MSGINFO arrives marks actual firmware loaded
            {
                state = LifetimeFNCState.FNCInitStart;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            else if(state == LifetimeFNCState.FNCInitStart && item.Type == SerialDataType.FNCEntryPrompt)
            {
                state = LifetimeFNCState.FNCInitFinish;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            else if(state == LifetimeFNCState.FNCInitFinish && item.Type == SerialDataType.MSGINFO)
            {
                state = LifetimeFNCState.FNCReady;
                OnFNCStateChanged(new FNCStateChangedEventArgs(state));
            }
            messages.Add(item);
            
        }

        public void Connect()
        {
            serial.SerialDataItemReceived += DataReceived;
            serial.Connect();
        }
    }
}
