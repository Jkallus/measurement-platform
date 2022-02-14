using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public class AxisState
    {
        public bool IsHomed { get; set; }
        public double? Position { get; set; }

        public AxisState()
        {
            IsHomed = false;
            Position = null;
        }

        public AxisState(bool isHomed, double position)
        {
            IsHomed = isHomed;
            Position = position;
        }
    }
}
