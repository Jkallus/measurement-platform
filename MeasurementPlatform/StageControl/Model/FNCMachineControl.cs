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
        private FluidNCController controller;
        private MachineState machineState;

        public LifetimeFNCState state
        {
            get { return controller.ControllerState; }
        }

        public FNCMachineControl()
        {
            controller = new FluidNCController();
            machineState = new MachineState();

            controller.FNCStateChanged += StateChanged;
            controller.ReceivedStatusUpdate += StatusUpdateReceived;
        }

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

        private void StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            Console.WriteLine(e.State.ToString());
        }

        private void StatusUpdateReceived(object? sender, StatusUpdateEventArgs e)
        {
            Console.WriteLine(e.Update.ToString());
        }

        public async Task<bool> Home()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            controller.RequestComplete += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            controller.Request(RequestType.HomeRequest);
            return await tcs.Task;
        }

    }
}
