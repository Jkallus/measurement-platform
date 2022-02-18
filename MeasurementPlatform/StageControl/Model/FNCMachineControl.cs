using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StageControl.Interfaces;
using StageControl.Events;

namespace StageControl.Model
{
    public class FNCMachineControl: IMachineControl
    {
        #region Private Members
        private FluidNCController controller;
        private MachineState state;

        #endregion

        #region Public Properties
        public event EventHandler<FNCStateChangedEventArgs>? StateChanged
        {
            add { this.controller.FNCStateChanged += value; }
            remove { this.controller.FNCStateChanged -= value;}
        }

        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        public double XPosition
        {
            get { return this.state.XAxis.Position.GetValueOrDefault(); }
        }

        public double YPosition
        {
            get { return this.state.YAxis.Position.GetValueOrDefault(); }
        }
        #endregion


        #region Constructors
        public FNCMachineControl()
        {
            controller = new FluidNCController();
            state = new MachineState();

            controller.FNCStateChanged += ReceivedStateChanged;
            controller.ReceivedStatusUpdate += StatusUpdateReceived;
        }

        public FNCMachineControl(SerialConfig serialConf, StageConfig stageConf)
        {
            controller = new FluidNCController(serialConf);
            state = new MachineState(stageConf);

            controller.FNCStateChanged += ReceivedStateChanged;
            controller.ReceivedStatusUpdate += StatusUpdateReceived;
        }

        #endregion

        #region Public Methods
        public async Task<bool> Initialize()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            controller.InitializationComplete += (sender, e) =>
            {
                tcs.SetResult(true);
            };

            controller.Connect();
            return await tcs.Task;
        }

        public void Deinitialize()
        {
            controller.Disconnect();
        }

        public async Task<bool> Home(HomingAxes axes)
        {
            return await Request(new HomingRequest(axes));
        }
        #endregion

        #region Private Methods
       
        private void ReceivedStateChanged(object? sender, FNCStateChangedEventArgs e) // event receiving code for inside the module
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




        protected virtual void OnPositionChanged(PositionChangedEventArgs e)
        {
            if(PositionChanged != null)
            {
                EventHandler<PositionChangedEventArgs> handler = PositionChanged;
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

            controller.Request(req);
            return await tcs.Task;
        }
        #endregion
    }
}