using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StageControl.Interfaces;
using StageControl.Events;
using MeasurementUI.Core.Models;
using Microsoft.Extensions.Logging;

namespace StageControl.Model
{
    public class FNCMachineControl: IMachineControl
    {
        #region Private Members
        private FluidNCController controller;
        private MachineState state;
        private readonly ILogger<FNCMachineControl> _logger;

        #endregion

        #region Public Properties
        public event EventHandler<FNCStateChangedEventArgs>? StateChanged
        {
            add => this.controller.FNCStateChanged += value;
            remove => this.controller.FNCStateChanged -= value;
        }

        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        public event EventHandler<RequestCompleteEventArgs>? RequestComplete
        {
            add => this.controller.RequestComplete += value;
            remove => this.controller.RequestComplete -= value;
        }

        public event EventHandler<RuntimeErrorEventArgs>? RuntimeError
        {
            add => this.controller.RuntimeError += value;
            remove => this.controller.RuntimeError -= value;
        }

        public event EventHandler<EventArgs>? RequestInProcess;

        public event EventHandler<EventArgs>? UnexpectedRestart
        {
            add => this.controller.UnexpectedRestart += value;
            remove => this.controller.UnexpectedRestart -= value;
        }

        public event EventHandler<EventArgs>? HomingComplete;

        public double XPosition => state.XAxis.Position.GetValueOrDefault();
        public double YPosition => state.YAxis.Position.GetValueOrDefault();
        public bool RequestPending => controller.RequestPending;
        public bool IsConnected => controller.IsConnected;
        public bool IsXAxisHomed => state.XAxis.IsHomed;
        public bool IsYAxisHomed => state.YAxis.IsHomed;
        public bool IsHomed => state.XAxis.IsHomed && state.YAxis.IsHomed;

        #endregion


        #region Constructors
        public FNCMachineControl(StageSerialConfig serialConf, StageConfig stageConf, ILogger<FNCMachineControl> topLogger, ILogger<FluidNCController> middleLogger, ILogger<SerialController> bottomLogger)
        {
            _logger = topLogger;
            controller = new FluidNCController(serialConf, middleLogger, bottomLogger);
            state = new MachineState(stageConf);
            controller.FNCStateChanged += Controller_StateChanged;
            controller.ReceivedStatusUpdate += StatusUpdateReceived;
            controller.RequestComplete += Controller_RequestComplete;
            _logger.LogInformation("FNCMachineControl constructed");
        }
        #endregion

        #region Public Methods
        public async Task<bool> Initialize()
        {
            _logger.LogInformation("Initializing Stage");
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            EventHandler<EventArgs>? InitCompleteEventHandler = null;
            InitCompleteEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
                controller.InitializationComplete -= InitCompleteEventHandler;
                _logger.LogInformation("Stage Initialization complete");
            };
            controller.InitializationComplete += InitCompleteEventHandler;
            controller.Connect();
            return await tcs.Task;
        }

        public void Deinitialize()
        {
            controller.Disconnect();
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
        #endregion

        #region Private Methods

        private void Controller_RequestComplete(object? sender, RequestCompleteEventArgs e)
        {
            if(e.Req != null)
            {
                if(e.Req is HomingRequest)
                {
                    HomingRequest? homingRequest = e.Req as HomingRequest;
                    if(homingRequest != null)
                    {
                        if (homingRequest.Axes == HomingAxes.XY)
                        {
                            state.XAxis.IsHomed = true;
                            state.YAxis.IsHomed = true;
                        }
                        else if (homingRequest.Axes == HomingAxes.X)
                        {
                            state.XAxis.IsHomed = true;
                        }
                        else if (homingRequest.Axes == HomingAxes.Y)
                        {
                            state.YAxis.IsHomed = true;
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

        private void Controller_StateChanged(object? sender, FNCStateChangedEventArgs e) // event receiving code for inside the module
        {
            //Console.WriteLine(e.State.ToString());
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
            if(RequestInProcess != null)
            {
                EventHandler<EventArgs> handler = RequestInProcess;
                handler?.Invoke(this, e);
            }
        }

        protected virtual void OnPositionChanged(PositionChangedEventArgs e)
        {
            if(PositionChanged != null)
            {
                EventHandler<PositionChangedEventArgs> handler = PositionChanged;
                handler?.Invoke(this, e);
            }
        }

        protected virtual void OnHomingComplete(EventArgs e)
        {
            if(HomingComplete != null)
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
                state.XAxis.Position = x;
                state.YAxis.Position = y;

                PositionChangedEventArgs args = new PositionChangedEventArgs(state.XAxis.Position.GetValueOrDefault(), state.YAxis.Position.GetValueOrDefault());
                OnPositionChanged(args);
            }
        }

        private void StatusUpdateReceived(object? sender, StatusUpdateEventArgs e)
        {
            //Console.WriteLine(e.Update.ToString());
            UpdateState(e);
        }

        private async Task<bool> Request(Request req)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            controller.RequestComplete += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            OnRequestInProcess(EventArgs.Empty);
            controller.Request(req);
            return await tcs.Task;
        }
        #endregion
    }
}