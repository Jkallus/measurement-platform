using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Events
{
    public class PositionChangedEventArgs: EventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PositionChangedEventArgs(double x, double y)
        {
            X = x;
            Y = y; 
        }
    }
}
