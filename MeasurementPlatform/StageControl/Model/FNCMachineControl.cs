using StageControl.Enums;
using StageControl.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public class FNCMachineControl: IMachineControl
    {
        #region Private Members
        private FluidNCController controller;
        private MachineState machineState;
        #endregion

        #region Public Properties
        public LifetimeFNCState state
        {
            get { return controller.ControllerState; }
        }
        #endregion

        #region Constructors
        public FNCMachineControl()
        {
            controller = new FluidNCController();
            machineState = new MachineState();

            controller.FNCStateChanged += StateChanged;
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
        private void StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            Console.WriteLine(e.State.ToString());
        }

        private void StatusUpdateReceived(object? sender, StatusUpdateEventArgs e)
        {
            Console.WriteLine(e.Update.ToString());
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