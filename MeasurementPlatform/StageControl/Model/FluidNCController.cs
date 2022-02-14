using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace StageControl.Model
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

    public class RequestCompleteEventArgs: EventArgs
    {

    }

    public class StatusUpdateEventArgs
    {
        public SerialDataItem Update { get; set; }

        public StatusUpdateEventArgs(SerialDataItem update)
        {
            Update = update;
        }

    }


    public class FluidNCController
    {

        #region Member Variables

        private readonly SerialController serial;
        private readonly List<SerialDataItem> incomingMessages;
        private readonly List<SerialDataItem> outgoingMessages;
        private readonly MachineState machineState;
        private readonly Timer statusTimer;

        private bool initPending;
        private bool requestPending;

        public event EventHandler<FNCStateChangedEventArgs>? FNCStateChanged;
        public event EventHandler<RequestCompleteEventArgs>? RequestComplete;
        public event EventHandler<StatusUpdateEventArgs>? ReceivedStatusUpdate;
        public event EventHandler<EventArgs>? InitializationComplete;        

        #endregion

        #region Public Properties
        private LifetimeFNCState controllerState;
        public LifetimeFNCState ControllerState
        {
            get { return controllerState; }
            private set { controllerState = value; }  
        }

        #endregion

        #region Constructors

        public FluidNCController()
        {
            serial = new SerialController();
            incomingMessages = new List<SerialDataItem>();
            outgoingMessages = new List<SerialDataItem>();
            machineState = new MachineState();
            initPending = false;
            requestPending = false;
            statusTimer = new Timer();
            initTimer();
            controllerState = LifetimeFNCState.Unknown;
            

        }
        #endregion

        #region Public Methods

        public void Request(RequestType req)
        {
            //activeRequest = req;
            requestPending = true;
            if(req == RequestType.HomeRequest)
            {   
                serial.SendSerialData("$HX");
                outgoingMessages.Add(new SerialDataItem("$HX", DateTime.Now, SerialDataType.OutgoingMessage));
            }
        }

        public void RequestStatus()
        {
            serial.SendStatusRequest();
        }

        public void Connect()
        {
            serial.SerialDataItemReceived += DataReceived;
            initPending = true;
            serial.Connect();
        }

        #endregion

        #region Event Handlers

        private void statusTimerTick(object? sender, EventArgs e)
        {
            RequestStatus();
        }

        private void DataReceived(object? sender, SerialDataItemReceivedEventArgs e)
        {
            if(e.Item != null)
            {
                ProcessIncomingSerialDataItem(e.Item);
                //Console.WriteLine(e.Item.ToString());
            }
        }

        #endregion

        #region Event Sources

        protected virtual void OnReceivedStatusUpdate(StatusUpdateEventArgs e)
        {
            if(ReceivedStatusUpdate != null)
            {
                EventHandler<StatusUpdateEventArgs> handler = ReceivedStatusUpdate;
                handler(this, e);
            }
        }

        protected virtual void OnInitializationComplete(EventArgs e)
        {
            if(InitializationComplete != null)
            {
                EventHandler<EventArgs> handler = InitializationComplete;
                handler(this, e);
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

        protected virtual void OnRequestComplete(RequestCompleteEventArgs e)
        {
            if(RequestComplete != null)
            {
                EventHandler<RequestCompleteEventArgs> handler = RequestComplete;
                handler(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void initTimer()
        {
            statusTimer.Interval = 1000;
            statusTimer.AutoReset = true;
            statusTimer.Elapsed += statusTimerTick;
        }

        private void ProcessIncomingSerialDataItem(SerialDataItem item)
        {
            if(controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.Status) // regular status update case, put first because most messages will be this
            {
                OnReceivedStatusUpdate(new StatusUpdateEventArgs(item));
            }
            if(controllerState == LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // expected transition when starting up
            {
                controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState != LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // if we get this at any other point that means a reset occured and FNC state is lost
            {
                controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState == LifetimeFNCState.FirstBoot && item.Type == SerialDataType.ESPBootloader) // next state in startup routine
            {
                controllerState = LifetimeFNCState.SecondBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState == LifetimeFNCState.SecondBoot && item.Type == SerialDataType.MSGINFO) // first MSGINFO arrives marks actual firmware loaded
            {
                controllerState = LifetimeFNCState.FNCInitStart;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState == LifetimeFNCState.FNCInitStart && item.Type == SerialDataType.FNCEntryPrompt)
            {
                controllerState = LifetimeFNCState.FNCInitFinish;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState == LifetimeFNCState.FNCInitFinish && item.Type == SerialDataType.MSGINFO)
            {
                controllerState = LifetimeFNCState.FNCReady;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
                if(initPending)
                {
                    OnInitializationComplete(new EventArgs());
                    initPending = false;
                }
                statusTimer.Enabled = true;
                    
            }
            else if(controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RequestComplete)
            {
                if (requestPending)
                {
                    OnRequestComplete(new RequestCompleteEventArgs());
                    requestPending = false;
                }
            }
            incomingMessages.Add(item);
            
        }

        #endregion
    }
}
