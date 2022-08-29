using StageControl.Enums;
using StageControl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementApp.Core.Models;
using Timer = System.Timers.Timer;
using Microsoft.Extensions.Logging;

namespace StageControl.Model;

public class FluidNCController
{
    // Private member variables
    private readonly SerialController _serial;
    private readonly List<SerialDataItem> _incomingMessages;
    private readonly List<SerialDataItem> _outgoingMessages;
    private readonly Timer _statusTimer;
    private readonly ILogger<FluidNCController> _logger;
    private bool _initPending;
    private Request? _activeRequest;
    private readonly double _compareThreshold;
    private readonly StageConfig _stageConfig;

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

    // Constructors
    public FluidNCController(SerialConfig serialConf, StageConfig stageConf, ILogger<FluidNCController> middleLogger, ILogger<SerialController> bottomLogger)
    {
        _logger = middleLogger;
        _stageConfig = stageConf;
        _serial = new SerialController(serialConf, bottomLogger);
        _incomingMessages = new List<SerialDataItem>();
        _outgoingMessages = new List<SerialDataItem>();
        _initPending = false;
        _requestPending = false;
        _isConnected = false;
        _compareThreshold = 0.0001;
        _statusTimer = new Timer();
        InitTimer();
        _controllerState = LifetimeFNCState.Unknown; // disconnected state is always Unknown
        _logger.LogInformation("FluidNCController constructed");
    }
    
    // Public methods
    public void Disconnect()
    {
        _statusTimer.Stop();
        _serial.SendSerialData("$MD");
        _serial.Disconnect();
        _controllerState = LifetimeFNCState.Unknown;
        OnFNCStateChanged(new FNCStateChangedEventArgs(ControllerState));
        IsConnected = false;
    }

    /// <summary>
    /// This synchronous function sends the message over serial that begins the move/operation
    /// </summary>
    public void Request(Request req)
    {
        _logger.LogInformation("Processing request");
        _requestPending = true;
        _activeRequest = req;
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
                _serial.SendSerialData(reqText);
                _outgoingMessages.Add(new SerialDataItem(reqText, DateTime.Now, SerialDataType.OutgoingMessage));
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
                reqText += $"F{_stageConfig.MaxSpeed} ";
                reqText += $"X{x_mm.ToString("0.000")} ";
                reqText += $"Y{y_mm.ToString("0.000")}";

                _serial.SendSerialData(reqText);
                _outgoingMessages.Add(new SerialDataItem(reqText, DateTime.Now, SerialDataType.OutgoingMessage));
            }
        }
        else if(req != null && req is MoveToRequest)
        {
            MoveToRequest moveToRequest = (MoveToRequest)req;
            if(moveToRequest != null)
            {
                string reqText = "G1 "; // Gcode linear move command
                reqText += " G90 "; // always absolute coordinates
                reqText += $"X{moveToRequest.X.ToString("0.000")} ";
                reqText += $"Y{moveToRequest.Y.ToString("0.000")} ";
                reqText += $"F{_stageConfig.MaxSpeed}";

                if(moveToRequest.Blocking == BlockingType.ExternallyBlocking)
                {
                    PositionChanged += FluidNCController_PositionChanged; // TODO should this be done before sending over serial?
                }

                _serial.SendSerialData(reqText);
                _outgoingMessages.Add(new SerialDataItem(reqText, DateTime.Now, SerialDataType.OutgoingMessage));
            }
        }

        //if(req!.Blocking == BlockingType.InternallyBlocking)
        //{

        //}
        //else if(req.Blocking == BlockingType.NonBlocking)
        //{

        //}
        //else if(req.Blocking == BlockingType.ExternallyBlocking)
        //{
            
        //}
    }

    public void Connect()
    {
        _logger.LogInformation("Connecting to FluidNC");
        _serial.SerialDataItemReceived += DataReceived;
        _initPending = true;
        _serial.Connect();
        IsConnected = true;
    }

    // Event handlers
    private void FluidNCController_PositionChanged(object? sender, PositionChangedEventArgs e)
    {
        if(_activeRequest != null && _requestPending)
        {
            if(_activeRequest is MoveToRequest && _activeRequest.Blocking == BlockingType.ExternallyBlocking)
            {
                MoveToRequest moveReq = (MoveToRequest)_activeRequest;
                if (PositionEquality(e.X, moveReq.X) && PositionEquality(e.Y, moveReq.Y))
                {
                    OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                    _activeRequest = null;
                    _requestPending = false;
                    PositionChanged -= FluidNCController_PositionChanged;
                }
            }
        }
    }

    private void statusTimerTick(object? sender, EventArgs e)
    {
        _logger.LogDebug("Sending Status Request");
        _serial.SendStatusRequest();
    }

    private void DataReceived(object? sender, SerialDataItemReceivedEventArgs e)
    {
        if(e.Item != null)
        {
            ProcessIncomingSerialDataItem(e.Item);
        }
    }

    // Event sources
    protected virtual void OnPositionChanged(PositionChangedEventArgs e)
    {
        if(PositionChanged != null)
        {
            EventHandler<PositionChangedEventArgs> handler = PositionChanged;
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
            _logger.LogInformation("FNC state changed: {state}", e.State);
            EventHandler<FNCStateChangedEventArgs> handler = FNCStateChanged;
            handler(this, e);
        }            
    }

    protected virtual void OnRequestComplete(RequestCompleteEventArgs e)
    {
        if(RequestComplete != null)
        {
            _logger.LogInformation("Request complete");
            EventHandler<RequestCompleteEventArgs> handler = RequestComplete;
            handler(this, e);
        }
    }

    protected virtual void OnUnexpectedRestart(EventArgs e)
    {
        if(UnexpectedRestart != null)
        {
            _logger.LogError("FluidNC Unexpected restart occured");
            EventHandler<EventArgs> handler = UnexpectedRestart;
            handler(this, e);
        }
    }

    protected virtual void OnRuntimeErrorReceived(RuntimeErrorEventArgs e)
    {
        if(RuntimeError != null)
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
        _statusTimer.Elapsed += statusTimerTick;
    }

    private void ProcessIncomingSerialDataItem(SerialDataItem item)
    {
        _logger.LogDebug("Received SerialDataItem {SDI}", item);
        if(_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.Status) // regular status update case, put first because most messages will be this
        {
            ProcessStatusUpdate(item);
        }
        else if(_controllerState == LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // expected transition when starting up
        {
            _controllerState = LifetimeFNCState.FirstBoot;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
        }
        else if(_controllerState != LifetimeFNCState.Unknown && item.Type == SerialDataType.ESPFirstBootMessage) // if we get this at any other point that means a reset occured and FNC state is lost
        {
            _controllerState = LifetimeFNCState.FirstBoot;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            OnUnexpectedRestart(EventArgs.Empty);
        }
        else if(_controllerState == LifetimeFNCState.FirstBoot && item.Type == SerialDataType.ESPBootloader) // next state in startup routine
        {
            _controllerState = LifetimeFNCState.SecondBoot;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
        }
        else if(_controllerState == LifetimeFNCState.SecondBoot && item.Type == SerialDataType.MSGINFO) // first MSGINFO arrives marks actual firmware loaded
        {
            _controllerState = LifetimeFNCState.FNCInitStart;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
        }
        else if(_controllerState == LifetimeFNCState.FNCInitStart && item.Type == SerialDataType.FNCEntryPrompt)
        {
            _controllerState = LifetimeFNCState.FNCInitFinish;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
        }
        else if(_controllerState == LifetimeFNCState.FNCInitFinish && item.Type == SerialDataType.MSGINFO)
        {
            _controllerState = LifetimeFNCState.FNCReady;
            OnFNCStateChanged(new FNCStateChangedEventArgs(_controllerState));
            if(_initPending)
            {
                OnInitializationComplete(new EventArgs());
                _initPending = false;
            }
            _statusTimer.Enabled = true;       
        }
        else if(_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RequestComplete)
        {
            ProcessRequestComplete();
        }
        else if(_controllerState == LifetimeFNCState.FNCReady && item.Type == SerialDataType.RuntimeError)
        {
            string message = _incomingMessages.Last().Data!;
            string[] parts = item.Data!.Split(':');
            int id = int.Parse(parts[1]);
            OnRuntimeErrorReceived(new RuntimeErrorEventArgs(message, id));
            if(_requestPending && _activeRequest != null) // TODO this needs to be looked at, possible incorrect behavior with ending the request like this
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest)); // Maybe here throw exception to be caught by TaskCompletionSource
                _requestPending = false;
                _activeRequest = null;
            }
        }
        _incomingMessages.Add(item);
    }

    private void ProcessRequestComplete()
    {
        if(_requestPending && _activeRequest != null)
        {
            if(_activeRequest.Blocking == BlockingType.InternallyBlocking) // internally blocking is simple, homing or other strictly blocking request is complete
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                _requestPending = false;
                _activeRequest = null;
            }
            else if(_activeRequest.Blocking == BlockingType.NonBlocking) // non blocking like Jog can return once "ok" is received even if the move isn't finished
            {
                OnRequestComplete(new RequestCompleteEventArgs(_activeRequest));
                _requestPending = false;
                _activeRequest = null;
            }
            else if(_activeRequest.Blocking == BlockingType.ExternallyBlocking) // Externally blocking for MoveTo returns when position reached, not when "ok" received
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
            string machineState = parts[0];
            string MPos = parts[1];
            string[] coordinateStrings = MPos.Substring(MPos.IndexOf(':') + 1).Split(',');
            double x = double.Parse(coordinateStrings[0]);
            double y = double.Parse(coordinateStrings[1]);
            double z = double.Parse(coordinateStrings[2]);
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
