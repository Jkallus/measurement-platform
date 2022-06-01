using StageControl.Enums;
using StageControl.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Interfaces
{
    public interface IMachineControl
    {
        event EventHandler<FNCStateChangedEventArgs>? StateChanged;
        event EventHandler<PositionChangedEventArgs>? PositionChanged;
        event EventHandler<RequestCompleteEventArgs>? RequestComplete;
        event EventHandler<EventArgs>? RequestInProcess;
        event EventHandler<EventArgs>? UnexpectedRestart;
        event EventHandler<EventArgs>? HomingComplete;
        event EventHandler<RuntimeErrorEventArgs>? RuntimeError;

        public Task<bool> Initialize();
        public void Deinitialize();
        public Task<bool> Home(HomingAxes axes);
        public Task<bool> Jog(int X, int Y, JogType type);

        public double XPosition { get; }
        public double YPosition { get; }

        public bool RequestPending { get; }
        public bool IsConnected { get; }
        public bool IsXAxisHomed { get; }
        public bool IsYAxisHomed { get; }
        public bool IsHomed { get; }
    }
}
