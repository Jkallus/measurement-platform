using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    public class FluidNCController
    {

        #region Member Variables

        private readonly SerialController serial;
        private readonly List<SerialDataItem> incomingMessages;
        private readonly List<SerialDataItem> outgoingMessages;
        private readonly MachineState machineState;

        private bool initPending;

        public event EventHandler<FNCStateChangedEventArgs>? FNCStateChanged;
        public event EventHandler<RequestCompleteEventArgs>? RequestComplete;
        public event EventHandler<EventArgs>? InitializationComplete;



        private RequestType activeRequest;

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

            controllerState = LifetimeFNCState.Unknown;
            activeRequest = RequestType.NoRequest;

        }
        #endregion

        #region Public Methods

        public void Request(RequestType req)
        {
            //activeRequest = req;
            if(req == RequestType.HomeRequest)
            {
                
                serial.SendSerialData("$HX");
                outgoingMessages.Add(new SerialDataItem("$HX", DateTime.Now, SerialDataType.OutgoingMessage));

            }
        }

        public void RequestStatus()
        {
            activeRequest = RequestType.StatusRequest;
            serial.SendSerialData("?");
        }

        public void Connect()
        {
            serial.SerialDataItemReceived += DataReceived;
            initPending = true;
            serial.Connect();
        }

        #endregion

        #region Event Handlers

        private void DataReceived(object? sender, SerialDataItemReceivedEventArgs e)
        {
            if(e.Item != null)
            {
                ProcessIncomingSerialDataItem(e.Item);
                Console.WriteLine(e.Item.ToString());
            }
        }

        #endregion

        #region Event Sources
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

        private void ProcessIncomingSerialDataItem(SerialDataItem item)
        {
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
                    
            }
            else if(controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RequestComplete)
            {
                OnRequestComplete(new RequestCompleteEventArgs());
            }
            incomingMessages.Add(item);
            
        }

        #endregion
    }
}
