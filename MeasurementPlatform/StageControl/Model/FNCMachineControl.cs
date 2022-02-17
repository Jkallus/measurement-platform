using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StageControl.Interfaces;
using StageControl.Core;
using StageControl.Core.Events;
using StageControl.Core.Enums;
using StageControl.Events;

namespace StageControl.Model
{
    public class FNCMachineControl: IMachineControl
    {
        #region Private Members
        private FluidNCController controller;
        private MachineState machineState;

        #endregion

        #region Public Properties
        public event EventHandler<FNCStateChangedEventArgs>? StateChanged
        {
            add { this.controller.FNCStateChanged += value; }
            remove { this.controller.FNCStateChanged -= value;}
        }
        #endregion


        #region Constructors
        public FNCMachineControl()
        {
            controller = new FluidNCController();
            machineState = new MachineState();

            controller.FNCStateChanged += ReceivedStateChanged;
            controller.ReceivedStatusUpdate += StatusUpdateReceived;
        }

        public FNCMachineControl(SerialConfig serialConf, StageConfig stageConf)
        {
            controller = new FluidNCController(serialConf);
            machineState = new MachineState();

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

        private void StatusUpdateReceived(object? sender, StatusUpdateEventArgs e)
        {
            //Console.WriteLine(e.Update.ToString());
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