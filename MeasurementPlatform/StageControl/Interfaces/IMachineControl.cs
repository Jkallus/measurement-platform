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

        public Task<bool> Initialize();
        public Task<bool> Home(HomingAxes axes);
    }
}
