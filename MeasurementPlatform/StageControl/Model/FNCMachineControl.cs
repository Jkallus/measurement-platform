using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StageControl.Interfaces;
using StageControl.Events;
using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;

namespace StageControl.Model
{
    public class FNCMachineControl: IMachineControl
    {
        // Private members
        private FluidNCController controller;
        private MachineState state;
        private readonly ILogger<FNCMachineControl> _logger;

        // Public properties
        public event EventHandler<FNCStateChangedEventArgs>? StateChanged
        {
            add => this.controller.FNCStateChanged += value;
            remove => this.controller.FNCStateChanged -= value;
        }

        public event EventHandler<PositionChangedEventArgs>? PositionChanged
        {
            add => this.controller.PositionChanged += value;
            remove => this.controller.PositionChanged -= value;
        }

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

        // Constructor
        public FNCMachineControl(StageSerialConfig serialConf, StageConfig stageConf, ILogger<FNCMachineControl> topLogger, ILogger<FluidNCController> middleLogger, ILogger<SerialController> bottomLogger)
        {
            _logger = topLogger;
            controller = new FluidNCController(serialConf, stageConf, middleLogger, bottomLogger);
            state = new MachineState(stageConf);
            controller.RequestComplete += Controller_RequestComplete;
            controller.PositionChanged += Controller_PositionChanged;
            _logger.LogInformation("FNCMachineControl constructed");
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
            state.XAxis.IsHomed = false;
            state.YAxis.IsHomed = false;
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
            state.XAxis.Position = e.X;
            state.YAxis.Position = e.Y;
        }

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

        protected virtual void OnRequestInProcess(EventArgs e)
        {
            if(RequestInProcess != null)
            {
                EventHandler<EventArgs> handler = RequestInProcess;
                handler(this, e);
            }
        }

        protected virtual void OnHomingComplete(EventArgs e)
        {
            if(HomingComplete != null)
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
                controller.RequestComplete -= RequestCompleteEventHandler;
                tcs.TrySetResult(true); // TODO add feedback here, don't always return true, area to add exception throwing
            };

            controller.RequestComplete += RequestCompleteEventHandler;
            OnRequestInProcess(EventArgs.Empty);
            controller.Request(req);
            return await tcs.Task;
        }
    }
}