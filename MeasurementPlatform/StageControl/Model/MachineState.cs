using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public class MachineState
    {
        public AxisState XAxis;
        public AxisState YAxis;

        public MachineState()
        {
            XAxis = new AxisState();
            YAxis = new AxisState();
        }       
    }
}
