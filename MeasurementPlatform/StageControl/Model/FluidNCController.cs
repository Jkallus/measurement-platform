using StageControl.Enums;
using StageControl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementUI.Core.Models;
using Timer = System.Timers.Timer;

namespace StageControl.Model
{
    
    public class FluidNCController
    {

        #region Member Variables

        private readonly SerialController serial;
        private readonly List<SerialDataItem> incomingMessages;
        private readonly List<SerialDataItem> outgoingMessages;
        private readonly Timer statusTimer;

        private bool initPending;
        private bool requestPending;
        private Request? activeRequest;
        private bool isConnected;

        public event EventHandler<FNCStateChangedEventArgs>? FNCStateChanged;
        public event EventHandler<RequestCompleteEventArgs>? RequestComplete;
        public event EventHandler<StatusUpdateEventArgs>? ReceivedStatusUpdate;
        public event EventHandler<EventArgs>? InitializationComplete;
        public event EventHandler<EventArgs>? UnexpectedRestart;
        public event EventHandler<RuntimeErrorEventArgs>? RuntimeError;

        #endregion

        #region Public Properties
        private LifetimeFNCState controllerState;
        public LifetimeFNCState ControllerState
        {
            get { return controllerState; }
            private set { controllerState = value; }  
        }

        public bool RequestPending { get => requestPending; }
        public bool IsConnected 
        {
            get => isConnected;
            private set => isConnected = value; 
        }

        #endregion

        #region Constructors

        public FluidNCController() : this(new SerialConfig("COM3"))
        {

        }

        public FluidNCController(SerialConfig serialConf)
        {
            serial = new SerialController(serialConf);
            incomingMessages = new List<SerialDataItem>();
            outgoingMessages = new List<SerialDataItem>();
            initPending = false;
            requestPending = false;
            isConnected = false;
            statusTimer = new Timer();
            initTimer();
            controllerState = LifetimeFNCState.Unknown;
        }
        #endregion

        #region Public Methods

        public void Disconnect()
        {
            statusTimer.Stop();
            serial.Disconnect();
            controllerState = LifetimeFNCState.Unknown;
            OnFNCStateChanged(new FNCStateChangedEventArgs(ControllerState));
            IsConnected = false;
        }

        public void Request(Request req)
        {
            requestPending = true;
            activeRequest = req;
            if(req != null && req is HomingRequest)
            {   
                HomingRequest homingRequest = (HomingRequest)req;
                if (homingRequest != null)
                {
                    string reqText = "";
                    if (homingRequest.Axes == HomingAxes.X)
                        reqText = "$HX";
                    else if (homingRequest.Axes == HomingAxes.Y)
                        reqText = "$HY";
                    else if (homingRequest.Axes == HomingAxes.XY)
                        reqText = "$H";
                    else
                    {
                        throw new Exception();
                    }
                    serial.SendSerialData(reqText);
                    outgoingMessages.Add(new SerialDataItem(reqText, DateTime.Now, SerialDataType.OutgoingMessage));
                }
            }
            else if(req != null && req is JogRequest)
            {
                JogRequest jogRequest = (JogRequest)req;
                if(jogRequest != null)
                {
                    string reqText = "$J=";
                    if (jogRequest.JogType == JogType.Absolute)
                        reqText += "G90 ";
                    else if (jogRequest.JogType == JogType.Incremental)
                        reqText += "G91 ";
                    else { }

                    float x_mm = (float)jogRequest.X / 1000;
                    float y_mm = (float)jogRequest.Y / 1000;

                    reqText += "G21 ";
                    reqText += "F700 ";
                    reqText += $"X{x_mm.ToString("0.000")} ";
                    reqText += $"Y{y_mm.ToString("0.000")}";

                    serial.SendSerialData(reqText);
                    outgoingMessages.Add(new SerialDataItem(reqText, DateTime.Now, SerialDataType.OutgoingMessage));
                }
            }
        }

        

        public void Connect()
        {
            serial.SerialDataItemReceived += DataReceived;
            initPending = true;
            serial.Connect();
            IsConnected = true;
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

        protected virtual void OnUnexpectedRestart(EventArgs e)
        {
            if(UnexpectedRestart != null)
            {
                 EventHandler<EventArgs> handler = UnexpectedRestart;
                handler(this, e);
            }
        }

        protected virtual void OnRuntimeErrorReceived(RuntimeErrorEventArgs e)
        {
            if(RuntimeError != null)
            {
                EventHandler<RuntimeErrorEventArgs> handler = RuntimeError;
                handler(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void RequestStatus()
        {
            serial.SendStatusRequest();
        }

        private void initTimer()
        {
            statusTimer.Interval = 500;
            statusTimer.AutoReset = true;
            statusTimer.Elapsed += statusTimerTick;
        }

        private void ProcessIncomingSerialDataItem(SerialDataItem item)
        {
            if(controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.Status) // regular status update case, put first because most messages will be this
            {
                OnReceivedStatusUpdate(new StatusUpdateEventArgs(item));
            }
            else if(controllerState == LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // expected transition when starting up
            {
                controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
            }
            else if(controllerState != LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // if we get this at any other point that means a reset occured and FNC state is lost
            {
                controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(controllerState));
                OnUnexpectedRestart(EventArgs.Empty);
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
                if (requestPending && activeRequest != null)
                {
                    OnRequestComplete(new RequestCompleteEventArgs(activeRequest));
                    requestPending = false;
                    activeRequest = null;

                }
            }
            else if(controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RuntimeError)
            {
                string message = incomingMessages.Last().Data!;
                string[] parts = item.Data!.Split(':');
                int id = int.Parse(parts[1]);
                OnRuntimeErrorReceived(new RuntimeErrorEventArgs(message, id));
                if(requestPending && activeRequest != null)
                {
                    OnRequestComplete(new RequestCompleteEventArgs(activeRequest));
                    requestPending = false;
                    activeRequest = null;
                }
            }
            incomingMessages.Add(item);
            
        }

        #endregion
    }
}
