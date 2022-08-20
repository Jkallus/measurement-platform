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
using MeasurementApp.Core.Models;

namespace StageControl.Model;

public class FluidNCSim
{
    // internal machine state
    private double _xLoc;
    private double _yLoc;
    private readonly double _xHomePosition;
    private readonly double _yHomePosition;

    private readonly ILogger<FluidNCSim> _logger;
    private readonly int _commandDelayMs;
    private readonly int _statusDelayMs;
    private readonly int _bootStepDelayMs;
    private readonly int _moveDelayMs;

    public event EventHandler<SerialDataItemReceivedEventArgs>? ResponseReceived;
    
    public FluidNCSim(ILogger<FluidNCSim> bottomLogger)
    {
        _logger = bottomLogger;
        _commandDelayMs = 50;
        _statusDelayMs = 10;
        _bootStepDelayMs = 100;
        _moveDelayMs = 2000;

        _xHomePosition = 298.000;
        _yHomePosition = 2.000;
        _xLoc = 150.000;
        _yLoc = 125.000;
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
            case MoveToRequest moveTo:
                HandleMoveTo(moveTo);
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
            OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(SerialDataConsts.RequestCompleteMessageMarker, DateTime.Now, SerialDataType.RequestComplete))); // send ack after processing but before moving
            await Task.Delay(_moveDelayMs);
            if(jogReq.JogType == JogType.Absolute)
            {
                _xLoc = jogReq.X / 1000.0;
                _yLoc = jogReq.Y / 1000.0;
            }
            else // incremental
            {
                _xLoc += jogReq.X / 1000.0;
                _yLoc += jogReq.Y / 1000.0;
            }
        });
    }

    private void HandleHoming(HomingRequest homeReq)
    {
        Task.Run(async () =>
        {
            await Task.Delay(_commandDelayMs); // delay to simulate FNC processing time
            await Task.Delay(_moveDelayMs);
            if (homeReq.Axes == HomingAxes.X)
                _xLoc = _xHomePosition;
            else if (homeReq.Axes == HomingAxes.Y)
                _yLoc = _yHomePosition;
            else
            {
                _xLoc = _xHomePosition;
                _yLoc = _yHomePosition;
            }
            OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(SerialDataConsts.RequestCompleteMessageMarker, DateTime.Now, SerialDataType.RequestComplete))); // send ack only after finishing
        });
    }

    private void HandleMoveTo(MoveToRequest moveTo)
    {
        Task.Run(async () =>
        {
            await Task.Delay(_commandDelayMs);
            OnResponseReceived(new SerialDataItemReceivedEventArgs(new SerialDataItem(SerialDataConsts.RequestCompleteMessageMarker, DateTime.Now, SerialDataType.RequestComplete))); // send ack after processing but before moving
            await Task.Delay(_moveDelayMs);
            _xLoc = moveTo.X;
            _yLoc = moveTo.Y;
        });
    }

    public void SendStatusRequest()
    {
        Task.Run(async () =>
        {
            await Task.Delay(_statusDelayMs);
            string statusMsg = $"<Alarm|MPos:{_xLoc.ToString("0.000")},{_yLoc.ToString("0.000")},0.000|FS:0,0>\n";
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
    // Private member variables
    private readonly FluidNCSim _fluidNCSim;
    private readonly List<SerialDataItem> _incomingMessages;
    private readonly List<SerialDataItem> _outgoingMessages;
    private readonly Timer _statusTimer;
    private readonly ILogger<FluidNCControllerSim> _logger;
    private bool _initPending;
    private Request? _activeRequest;
    private readonly double _compareThreshold;
    
    // Public properties
    public event EventHandler<FNCStateChangedEventArgs>? FNCStateChanged;
    public event EventHandler<RequestCompleteEventArgs>? RequestComplete;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    public event EventHandler<EventArgs>? InitializationComplete;
    public event EventHandler<EventArgs>? UnexpectedRestart;
    public event EventHandler<RuntimeErrorEventArgs>? RuntimeError;

    private LifetimeFNCState _controllerState;
    public LifetimeFNCState ControllerState
    {
        get => _controllerState;
        private set => _controllerState = value;
    }

    private bool _requestPending;
    public bool RequestPending => _requestPending;

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set => _isConnected = value;
    }

    // Constructor
    public FluidNCControllerSim(ILogger<FluidNCControllerSim> middleLogger, ILogger<FluidNCSim> bottomLogger)
    {
        _logger = middleLogger;
        _fluidNCSim = new FluidNCSim(bottomLogger);
        _incomingMessages = new List<SerialDataItem>();
        _outgoingMessages = new List<SerialDataItem>();
        _initPending = false;
        _requestPending = false;
        _isConnected = false;
        _compareThreshold = 0.0001;
        _statusTimer = new Timer();
        InitTimer();
        _controllerState = LifetimeFNCState.Unknown;
        _logger.LogInformation("FluidNCControllerSim constructed");
    }

    // Public methods
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
        _requestPending = true;
        _activeRequest = req;
        if(req != null && req is HomingRequest)
        {
            HomingRequest homingRequest = (HomingRequest)req;
            if(homingRequest != null)
            {
                _fluidNCSim.ReceiveRequest(req);
                _outgoingMessages.Add(new SerialDataItem(req.ToString()!, DateTime.Now, SerialDataType.OutgoingMessage));
            }
        }
        else if(req != null && req is JogRequest)
        {
            JogRequest jogRequest = (JogRequest)req;
            if(jogRequest != null)
            {
                _fluidNCSim.ReceiveRequest(req);
                _outgoingMessages.Add(new SerialDataItem(req.ToString()!, DateTime.Now, SerialDataType.OutgoingMessage));
            }
        }
        else if(req != null && req is MoveToRequest)
        {
            MoveToRequest moveToRequest = (MoveToRequest)req;
            if(moveToRequest != null)
            {
                PositionChanged += FluidNCControllerSim_PositionChanged;
                _fluidNCSim.ReceiveRequest(req);
            }
        }

        if(req!.Blocking == BlockingType.ExternallyBlocking)
        {
            
        }
    }

    public void Connect()
    {
        _logger.LogInformation("Connecting to FluidNCSim");
        _fluidNCSim.ResponseReceived += ResponseReceived;
        _initPending = true;
        _fluidNCSim.Connect();
        IsConnected = true; // TODO this should be refactored because its not actually connected till connection is confirmed,
                            // maybe rename to show what it actually represents
    }

    // Event handlers
    private void FluidNCControllerSim_PositionChanged(object? sender, PositionChangedEventArgs e)
    {
        if (_activeRequest != null && _requestPending)
        {
            if (_activeRequest is MoveToRequest && _activeRequest.Blocking == BlockingType.ExternallyBlocking)
            {
                MoveToRequest moveReq = (MoveToRequest)_activeRequest;
                if (PositionEquality(e.X, moveReq.X) && PositionEquality(e.Y, moveReq.Y))
                {
                    OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                    _activeRequest = null;
                    _requestPending = false;
                    PositionChanged -= FluidNCControllerSim_PositionChanged;
                }
                else
                {
                    double[] param =  { e.X, e.Y, moveReq.X, moveReq.Y};
                    _logger.LogInformation("Position not reached yet. Current Position"); // ({CurrX},{CurrY}), Target Position({TargX},{TargY})", (object)param);
                }
            }
        }
    }
    private void StatusTimerTick(object? sender, EventArgs e)
    {
        _logger.LogDebug("Sending Status Request");
        _fluidNCSim.SendStatusRequest();
    }

    private void ResponseReceived(object? sender, SerialDataItemReceivedEventArgs e)
    {
        if(e.Item != null)
        {
            ProcessIncomingSerialDataItem(e.Item);
        }
    }

    protected virtual void OnPositionChanged(PositionChangedEventArgs e)
    {
        if (PositionChanged != null)
        {
            EventHandler<PositionChangedEventArgs> handler = PositionChanged;
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

    // Private methods
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
            ProcessStatusUpdate(item);
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
            if (_initPending)
            {
                OnInitializationComplete(new EventArgs());
                _initPending = false;
            }
            _statusTimer.Enabled = true;
        }
        else if (_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RequestComplete)
        {
            ProcessRequestComplete();
        }
        else if (_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RuntimeError)
        {
            string message = _incomingMessages.Last().Data!;
            string[] parts = item.Data!.Split(':');
            int id = int.Parse(parts[1]);
            OnRuntimeErrorReceived(new RuntimeErrorEventArgs(message, id));
            if (_requestPending && _activeRequest != null)
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                _requestPending = false;
                _activeRequest = null;
            }
        }
        _incomingMessages.Add(item);
    }

    private void ProcessRequestComplete()
    {
        if (_requestPending && _activeRequest != null)
        {
            if (_activeRequest.Blocking == BlockingType.InternallyBlocking) // internally blocking is simple, homing or other strictly blocking request is complete
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                _requestPending = false;
                _activeRequest = null;
            }
            else if (_activeRequest.Blocking == BlockingType.NonBlocking) // non blocking like Jog can return once "ok" is received even if the move isn't finished
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                _requestPending = false;
                _activeRequest = null;
            }
            else if (_activeRequest.Blocking == BlockingType.ExternallyBlocking) // Externally blocking for MoveTo returns when position reached, not when "ok" received
            {
                // If externally blocking then request complete will happen in PositionChanged handler inside FluidNCController
            }
        }
    }

    private void ProcessStatusUpdate(SerialDataItem item)
    {
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
        if (item.Data != null)
        {
            string data = item.Data;
            data = data.Trim(new char[] { '<', '>' });
            string[] parts = data.Split('|');
            string MPos = parts[1];
            string[] coordinateStrings = MPos.Substring(MPos.IndexOf(':') + 1).Split(',');
            double x = double.Parse(coordinateStrings[0]);
            double y = double.Parse(coordinateStrings[1]);
            PositionChangedEventArgs args = new PositionChangedEventArgs(x, y);
            OnPositionChanged(args);
        }
    }

    private bool PositionEquality(double a, double b)
    {
        if (Math.Abs(a - b) < _compareThreshold)
            return true;
        else
            return false;
    }
}

public class FNCMachineControlSim : IMachineControl
{
    // Private members
    private readonly FluidNCControllerSim _controller;
    private readonly MachineState _state;
    private readonly ILogger<FNCMachineControlSim> _logger;
    
    // Public properties
    public event EventHandler<FNCStateChangedEventArgs>? StateChanged
    {
        add => this._controller.FNCStateChanged += value;
        remove => this._controller.FNCStateChanged -= value;
    }

    public event EventHandler<PositionChangedEventArgs>? PositionChanged
    {
        add => this._controller.PositionChanged += value;
        remove => this._controller.PositionChanged -= value;
    }

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
    public double XPosition => _state.XAxis.Position.GetValueOrDefault();
    public double YPosition => _state.YAxis.Position.GetValueOrDefault();
    public bool RequestPending => _controller.RequestPending;
    public bool IsConnected => _controller.IsConnected;
    public bool IsXAxisHomed => _state.XAxis.IsHomed;
    public bool IsYAxisHomed => _state.YAxis.IsHomed;
    public bool IsHomed => _state.XAxis.IsHomed && _state.YAxis.IsHomed;

    
    // Constructor
    public FNCMachineControlSim(StageConfig stageConf, ILogger<FNCMachineControlSim> topLogger, ILogger<FluidNCControllerSim> middleLogger, ILogger<FluidNCSim> bottomLogger)
    {
        _logger = topLogger;
        _controller = new FluidNCControllerSim(middleLogger, bottomLogger);
        _state = new MachineState(stageConf);
        _controller.PositionChanged += Controller_PositionChanged;
        _controller.RequestComplete += Controller_RequestComplete;
        _logger.LogInformation("FNCMachineControlSim constructed");
    }

    // Public methods
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

    public async Task<bool> MoveTo(double X, double Y)
    {
        return await Request(new MoveToRequest(X, Y, BlockingType.ExternallyBlocking));
    }

    public async Task<bool> MoveToNonBlocking(double X, double Y)
    {
        return await Request(new MoveToRequest(X, Y, BlockingType.NonBlocking));
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

    // Private methods
    private void Controller_PositionChanged(object? sender, PositionChangedEventArgs e)
    {
        _state.XAxis.Position = e.X;
        _state.YAxis.Position = e.Y;
    }

    private void Controller_RequestComplete(object? sender, RequestCompleteEventArgs e)
    {
        if (e.Req != null)
        {
            if (e.Req is HomingRequest homingRequest)
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
        else
        {
            throw new NullReferenceException();
        }
    }

    protected virtual void OnRequestInProcess(EventArgs e)
    {
        if (RequestInProcess != null)
        {
            EventHandler<EventArgs> handler = RequestInProcess;
            handler(this, e);
        }
    }

    protected virtual void OnHomingComplete(EventArgs e)
    {
        if (HomingComplete != null)
        {
            EventHandler<EventArgs> handler = HomingComplete;
            handler(this, e);
        }
    }

    private async Task<bool> Request(Request req)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        EventHandler<RequestCompleteEventArgs>? RequestCompleteEventHandler = null;
        RequestCompleteEventHandler = (sender, e) =>
        {
            _controller.RequestComplete -= RequestCompleteEventHandler;
            tcs.TrySetResult(true);
        };
        _controller.RequestComplete += RequestCompleteEventHandler;
        OnRequestInProcess(EventArgs.Empty);
        _controller.Request(req);
        return await tcs.Task;
    }
}