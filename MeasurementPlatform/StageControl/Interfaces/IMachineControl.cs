using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Interfaces
{
    public interface IMachineControl
    {
        public Task<bool> Initialize();
        public Task<bool> Home(HomingAxes axes);
    }
}
