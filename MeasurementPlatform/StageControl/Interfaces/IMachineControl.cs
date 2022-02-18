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

        public Task<bool> Initialize();
        public Task<bool> Home(HomingAxes axes);

        public double XPosition { get; }
        public double YPosition { get; }
    }
}
