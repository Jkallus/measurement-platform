using Microsoft.Extensions.Logging;
using StageControl.Enums;
using StageControl.Events;
using StageControl.Interfaces;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StageControl.Consts;
using Timer = System.Timers.Timer;

namespace StageControl.Model
{
    public class FluidNCSim
    {
        private ILogger<FluidNCSim> _logger;
        private int _commandDelayMs;
        private int _statusDelayMs;
        private int _bootStepDelayMs;

        public event EventHandler<SerialDataItemReceivedEventArgs>? ResponseReceived;
        

        public FluidNCSim(ILogger<FluidNCSim> bottomLogger)
        {
            _logger = bottomLogger;
            _commandDelayMs = 50;
            _statusDelayMs = 10;
            _bootStepDelayMs = 100;
        }

        public void ReceiveRequest(Request req)
        {
            switch(req)
            {
                case JogRequest jogReq:
                    HandleJog(jogReq);
                    break;
                case HomingRequest homeReq:
                    HandleHoming(homeReq);
                    break;
                default:
                    throw new Exception("Invalid request type");
            }
        }

        private void HandleJog(JogRequest jogReq)
        {
            Task.Run(async () =>
            {
                await Task.Delay(_commandDelayMs);
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(SerialDataConsts.RequestCompleteMessageMarker, DateTime.Now, SerialDataType.RequestComplete)));
            });
        }

        private void HandleHoming(HomingRequest homeReq)
        {
            Task.Run(async () =>
            {
                await Task.Delay(_commandDelayMs);
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(SerialDataConsts.RequestCompleteMessageMarker, DateTime.Now, SerialDataType.RequestComplete)));
            });
        }

        public void SendStatusRequest()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_statusDelayMs);
                float x = 0.000f;
                float y = 0.000f;
                string statusMsg = $"<Alarm|MPos:{x.ToString("0.000")},{y.ToString("0.000")},0.000|FS:0,0>\n";
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(statusMsg, DateTime.Now, SerialDataType.Status)));
            });
        }

        internal void Connect()
        {
            TriggerReboot();
        }

        private void TriggerReboot()
        {
            Task.Run(async () =>
            {
                // fake out initialization routine with simulated messages for FNC initialization

                // phase 1
                await Task.Delay(_bootStepDelayMs);
                string data = SerialDataConsts.ESPFirstBootMessageMarker + SerialDataConsts.DoubleLineBreak;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.ESPFirstBootMessage)));

                // phase 2
                await Task.Delay(_bootStepDelayMs);
                data = SerialDataConsts.ESPBootloaderMessageMarker + SerialDataConsts.DoubleLineBreak;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.ESPBootloader)));

                // phase 3
                await Task.Delay(_bootStepDelayMs);
                data = SerialDataConsts.MSGINFOMessageMarker + SerialDataConsts.MSGEndMarker;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.MSGINFO)));

                // phase 4
                await Task.Delay(_bootStepDelayMs);
                data = SerialDataConsts.MSGINFOMessageMarker + SerialDataConsts.MSGEndMarker;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.MSGINFO)));

                // phase 5
                await Task.Delay(_bootStepDelayMs);
                data = SerialDataConsts.FNCEntryPromptMessageMarker + SerialDataConsts.LineBreak;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.FNCEntryPrompt)));

                // phase 6
                await Task.Delay(_bootStepDelayMs);
                data = SerialDataConsts.MSGINFOMessageMarker + SerialDataConsts.MSGEndMarker;
                OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(data, DateTime.Now, SerialDataType.MSGINFO)));

            });
        }

        protected virtual void OnResponseReceived(SerialDataItemReceivedEventArgs e)
        {
            if(ResponseReceived != null)
            {
                EventHandler<SerialDataItemReceivedEventArgs> handler = ResponseReceived;
                handler(this, e);
            }
        }
    }


    public class FluidNCControllerSim
    {
        private readonly FluidNCSim _fluidNCSim;
        private readonly List<SerialDataItem> incomingMessages;
        private readonly List<SerialDataItem> outgoingMessages;
        private readonly Timer _statusTimer;
        private readonly ILogger<FluidNCControllerSim> _logger;
        private bool initPending;
        
        private Request? activeRequest;
        

        public event EventHandler<FNCStateChangedEventArgs>? FNCStateChanged;
        public event EventHandler<RequestCompleteEventArgs>? RequestComplete;
        public event EventHandler<StatusUpdateEventArgs>? ReceivedStatusUpdate;
        public event EventHandler<EventArgs>? InitializationComplete;
        public event EventHandler<EventArgs>? UnexpectedRestart;
        public event EventHandler<RuntimeErrorEventArgs>? RuntimeError;

        private LifetimeFNCState __controllerState;
        public LifetimeFNCState _controllerState
        {
            get { return __controllerState; }
            private set { __controllerState = value; }
        }

        private bool requestPending;
        public bool RequestPending { get => requestPending; }

        private bool isConnected;
        public bool IsConnected
        {
            get => isConnected;
            private set => isConnected = value;
        }

        public FluidNCControllerSim(ILogger<FluidNCControllerSim> middleLogger, ILogger<FluidNCSim> bottomLogger)
        {
            _logger = middleLogger;
            _fluidNCSim = new FluidNCSim(bottomLogger);
            incomingMessages = new List<SerialDataItem>();
            outgoingMessages = new List<SerialDataItem>();
            initPending = false;
            requestPending = false;
            isConnected = false;
            _statusTimer = new Timer();
            InitTimer();
            __controllerState = LifetimeFNCState.Unknown;
            _logger.LogInformation("FluidNCControllerSim constructed");
        }

        public void Disconnect()
        {
            _statusTimer.Stop();
            // TODO add simulated disconnect
            _controllerState = LifetimeFNCState.Unknown;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            IsConnected = false;
        }

        public void Request(Request req)
        {
            _logger.LogInformation("Processing request");
            requestPending = true;
            activeRequest = req;
            if(req != null && req is HomingRequest)
            {
                HomingRequest homingRequest = (HomingRequest)req;
                if(homingRequest != null)
                {
                    _fluidNCSim.ReceiveRequest(req);
                    outgoingMessages.Add(new SerialDataItem(req.ToString(), DateTime.Now, SerialDataType.OutgoingMessage));
                }
            }
            else if(req != null && req is JogRequest)
            {
                JogRequest jogRequest = (JogRequest)req;
                if(jogRequest != null)
                {
                    _fluidNCSim.ReceiveRequest(req);
                    outgoingMessages.Add(new SerialDataItem(req.ToString(), DateTime.Now, SerialDataType.OutgoingMessage));
                }
            }
        }

        public void Connect()
        {
            _logger.LogInformation("Connecting to FluidNCSim");
            _fluidNCSim.ResponseReceived += ResponseReceived;
            initPending = true;
            _fluidNCSim.Connect();
            IsConnected = true; // TODO this should be refactored because its not actually connected till connection is confirmed,
                                // maybe rename to show what it actually represents
        }


        private void StatusTimerTick(object? sender, EventArgs e)
        {
            RequestStatus();
        }

        private void ResponseReceived(object? sender, SerialDataItemReceivedEventArgs e)
        {
            if(e.Item != null)
            {
                ProcessIncomingSerialDataItem(e.Item);
            }
        }

        protected virtual void OnReceivedStatusUpdate(StatusUpdateEventArgs e)
        {
            if (ReceivedStatusUpdate != null)
            {
                EventHandler<StatusUpdateEventArgs> handler = ReceivedStatusUpdate;
                handler(this, e);
            }
        }

        protected virtual void OnInitializationComplete(EventArgs e)
        {
            if (InitializationComplete != null)
            {
                EventHandler<EventArgs> handler = InitializationComplete;
                handler(this, e);
            }
        }

        protected virtual void OnFNCStateChanged(FNCStateChangedEventArgs e)
        {
            if (FNCStateChanged != null)
            {
                _logger.LogInformation("FNC state changed: {state}", e.State);
                EventHandler<FNCStateChangedEventArgs> handler = FNCStateChanged;
                handler(this, e);
            }
        }

        protected virtual void OnRequestComplete(RequestCompleteEventArgs e)
        {
            if (RequestComplete != null)
            {
                _logger.LogInformation("Request complete");
                EventHandler<RequestCompleteEventArgs> handler = RequestComplete;
                handler(this, e);
            }
        }

        protected virtual void OnUnexpectedRestart(EventArgs e)
        {
            if (UnexpectedRestart != null)
            {
                _logger.LogError("FluidNC Unexpected restart occured");
                EventHandler<EventArgs> handler = UnexpectedRestart;
                handler(this, e);
            }
        }

        protected virtual void OnRuntimeErrorReceived(RuntimeErrorEventArgs e)
        {
            if (RuntimeError != null)
            {
                _logger.LogWarning("Stage runtime error: [{Id}] {Error}", e.Error, e.Message);
                EventHandler<RuntimeErrorEventArgs> handler = RuntimeError;
                handler(this, e);
            }
        }


        private void RequestStatus()
        {
            _fluidNCSim.SendStatusRequest();
        }

        private void InitTimer()
        {
            _statusTimer.Interval = 200;
            _statusTimer.AutoReset = true;
            _statusTimer.Elapsed += StatusTimerTick;
        }

        private void ProcessIncomingSerialDataItem(SerialDataItem item)
        {
            _logger.LogDebug("Received SerialDataItem {SDI}", item);
            if (_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.Status) // regular status update case, put first because most messages will be this
            {
                OnReceivedStatusUpdate(new StatusUpdateEventArgs(item));
            }
            else if (_controllerState == LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // expected transition when starting up
            {
                _controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            }
            else if (_controllerState != LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // if we get this at any other point that means a reset occured and FNC state is lost
            {
                _controllerState = LifetimeFNCState.FirstBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
                OnUnexpectedRestart(EventArgs.Empty);
            }
            else if (_controllerState == LifetimeFNCState.FirstBoot && item.Type == SerialDataType.ESPBootloader) // next state in startup routine
            {
                _controllerState = LifetimeFNCState.SecondBoot;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            }
            else if (_controllerState == LifetimeFNCState.SecondBoot && item.Type == SerialDataType.MSGINFO) // first MSGINFO arrives marks actual firmware loaded
            {
                _controllerState = LifetimeFNCState.FNCInitStart;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            }
            else if (_controllerState == LifetimeFNCState.FNCInitStart && item.Type == SerialDataType.FNCEntryPrompt)
            {
                _controllerState = LifetimeFNCState.FNCInitFinish;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            }
            else if (_controllerState == LifetimeFNCState.FNCInitFinish && item.Type == SerialDataType.MSGINFO)
            {
                _controllerState = LifetimeFNCState.FNCReady;
                OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
                if (initPending)
                {
                    OnInitializationComplete(new EventArgs());
                    initPending = false;
                }
                _statusTimer.Enabled = true;
            }
            else if (_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RequestComplete)
            {
                if (requestPending && activeRequest != null)
                {
                    OnRequestComplete(new RequestCompleteEventArgs(activeRequest));
                    requestPending = false;
                    activeRequest = null;

                }
            }
            else if (_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RuntimeError)
            {
                string message = incomingMessages.Last().Data!;
                string[] parts = item.Data!.Split(':');
                int id = int.Parse(parts[1]);
                OnRuntimeErrorReceived(new RuntimeErrorEventArgs(message, id));
                if (requestPending && activeRequest != null)
                {
                    OnRequestComplete(new RequestCompleteEventArgs(activeRequest));
                    requestPending = false;
                    activeRequest = null;
                }
            }
            incomingMessages.Add(item);
        }

    }

    public class FNCMachineControlSim : IMachineControl
    {
        private MachineState _state;
        private readonly ILogger<FNCMachineControlSim> _logger;
        private FluidNCControllerSim _controller;

        public double XPosition => _state.XAxis.Position.GetValueOrDefault();
        public double YPosition => _state.YAxis.Position.GetValueOrDefault();
        public bool RequestPending => _controller.RequestPending;
        public bool IsConnected => _controller.IsConnected;
        public bool IsXAxisHomed => _state.XAxis.IsHomed;
        public bool IsYAxisHomed => _state.YAxis.IsHomed;
        public bool IsHomed => _state.XAxis.IsHomed && _state.YAxis.IsHomed;

        public event EventHandler<FNCStateChangedEventArgs>? StateChanged
        {
            add => this._controller.FNCStateChanged += value;
            remove => this._controller.FNCStateChanged -= value;
        }

        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        public event EventHandler<RequestCompleteEventArgs>? RequestComplete
        {
            add => this._controller.RequestComplete += value;
            remove => this._controller.RequestComplete -= value;
        }

        public event EventHandler<RuntimeErrorEventArgs>? RuntimeError
        {
            add => this._controller.RuntimeError += value;
            remove => this._controller.RuntimeError -= value;
        }

        public event EventHandler<EventArgs>? RequestInProcess;

        
        public event EventHandler<EventArgs>? UnexpectedRestart
        {
            add => this._controller.UnexpectedRestart += value;
            remove => this._controller.UnexpectedRestart -= value;
        }

        public event EventHandler<EventArgs>? HomingComplete;
        
        public FNCMachineControlSim(StageConfig stageConf, ILogger<FNCMachineControlSim> topLogger, ILogger<FluidNCControllerSim> middleLogger, ILogger<FluidNCSim> bottomLogger)
        {
            _logger = topLogger;
            _controller = new FluidNCControllerSim(middleLogger, bottomLogger);
            _state = new MachineState(stageConf);
            //_controller.FNCStateChanged += Controller_StateChanged;
            _controller.ReceivedStatusUpdate += StatusUpdateReceived;
            _controller.RequestComplete += Controller_RequestComplete;
            _logger.LogInformation("FNCMachineControlSim constructed");
        }

        private void Controller_RequestComplete(object? sender, RequestCompleteEventArgs e)
        {
            if (e.Req != null)
            {
                if (e.Req is HomingRequest)
                {
                    HomingRequest? homingRequest = e.Req as HomingRequest;
                    if (homingRequest != null)
                    {
                        if (homingRequest.Axes == HomingAxes.XY)
                        {
                            _state.XAxis.IsHomed = true;
                            _state.YAxis.IsHomed = true;
                        }
                        else if (homingRequest.Axes == HomingAxes.X)
                        {
                            _state.XAxis.IsHomed = true;
                        }
                        else if (homingRequest.Axes == HomingAxes.Y)
                        {
                            _state.YAxis.IsHomed = true;
                        }
                        OnHomingComplete(new EventArgs());
                        _logger.LogInformation("Homing complete");
                    }

                }
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        

        public static string ConvertControllerStateToStatus(LifetimeFNCState state)
        {
            switch (state)
            {
                case LifetimeFNCState.Unknown:
                    return "Unknown State";
                case LifetimeFNCState.FirstBoot:
                    return "Booting Stage 1";
                case LifetimeFNCState.SecondBoot:
                    return "Booting Stage 2";
                case LifetimeFNCState.FNCInitStart:
                    return "Initialization Start";
                case LifetimeFNCState.FNCInitFinish:
                    return "Initialization Finish";
                case LifetimeFNCState.FNCReady:
                    return "Online";
                default:
                    return "Other";

            }
        }

        protected virtual void OnRequestInProcess(EventArgs e)
        {
            if (RequestInProcess != null)
            {
                EventHandler<EventArgs> handler = RequestInProcess;
                handler?.Invoke(this, e);
            }
        }

        protected virtual void OnPositionChanged(PositionChangedEventArgs e)
        {
            if (PositionChanged != null)
            {
                EventHandler<PositionChangedEventArgs> handler = PositionChanged;
                handler?.Invoke(this, e);
            }
        }

        protected virtual void OnHomingComplete(EventArgs e)
        {
            if (HomingComplete != null)
            {
                EventHandler<EventArgs> handler = HomingComplete;
                handler?.Invoke(this, e);
            }
        }

        /*
         * Possible Strings
         * <Idle|MPos:2.000,2.000,0.000|FS:0,0>
         * <Idle|MPos:2.000,2.000,0.000|FS:0,0|Ov:100,100,100>
         * <Idle|MPos:2.000,2.000,0.000|FS:0,0|WCO:0.000,0.000,0.000>
         * <Home|MPos:2.000,1.889,0.000|FS:100,0|Ov:100,100,100>
         * <Home|MPos:2.000,1.736,0.000|FS:100,0|WCO:0.000,0.000,0.000>
         * <Home|MPos:2.000,-1.922,0.000|FS:100,0|Pn:Y>
         * <Home|MPos:0.186,0.000,0.000|FS:100,0|Pn:X>
         * */
        private void UpdateState(StatusUpdateEventArgs e)
        {
            if (e.Update.Data != null)
            {
                string data = e.Update.Data;
                data = data.Trim(new char[] { '<', '>' });
                string[] parts = data.Split('|');
                string machineState = parts[0];
                string MPos = parts[1];
                string[] coordinateStrings = MPos.Substring(MPos.IndexOf(':') + 1).Split(',');
                double x = double.Parse(coordinateStrings[0]);
                double y = double.Parse(coordinateStrings[1]);
                double z = double.Parse(coordinateStrings[2]);
                _state.XAxis.Position = x;
                _state.YAxis.Position = y;

                PositionChangedEventArgs args = new PositionChangedEventArgs(_state.XAxis.Position.GetValueOrDefault(), _state.YAxis.Position.GetValueOrDefault());
                OnPositionChanged(args);
            }
        }

        private void StatusUpdateReceived(object? sender, StatusUpdateEventArgs e)
        {
            UpdateState(e);
        }

        private async Task<bool> Request(Request req)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            _controller.RequestComplete += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            OnRequestInProcess(EventArgs.Empty);
            _controller.Request(req);
            return await tcs.Task;
        }

        //private void Controller_StateChanged(object? sender, FNCStateChangedEventArgs e)
        //{

        //}

        public void Deinitialize()
        {
            _controller.Disconnect();
            _state.XAxis.IsHomed = false;
            _state.YAxis.IsHomed = false;
            _logger.LogInformation("Deinitialized stage");
        }

        public async Task<bool> Jog(int X, int Y, JogType type)
        {
            return await Request(new JogRequest(X, Y, type));
        }

        public async Task<bool> Home(HomingAxes axes)
        {
            return await Request(new HomingRequest(axes));
        }

        public async Task<bool> Initialize()
        {
            _logger.LogInformation("Initializing Stage");
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            EventHandler<EventArgs>? InitCompleteEventHandler = null;
            InitCompleteEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
                _controller.InitializationComplete -= InitCompleteEventHandler;
                _logger.LogInformation("Stage Initialization complete");
            };
            _controller.InitializationComplete += InitCompleteEventHandler;
            _controller.Connect();
            return await tcs.Task;
        }

        
    }
}
