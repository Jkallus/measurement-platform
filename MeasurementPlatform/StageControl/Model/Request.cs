using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public enum BlockingType
    {
        InternallyBlocking, // $H is internally blocking, it only replys "ok" when homing is complete
        NonBlocking, // $J is non blocking at the FluidNC level, it returns "ok" as soon as the command is parsed
        ExternallyBlocking, // G1 and other Gcode commands are non blocking at the FluidNC level however they need to be blocking for MAM scanning
    }

    public abstract class Request
    {
        public BlockingType Blocking { get; set; }
    }

    public class HomingRequest: Request
    {
        public HomingAxes Axes { get; set; }   

        public HomingRequest(HomingAxes axes)
        {
            Blocking = BlockingType.InternallyBlocking;
            Axes = axes;   
        }
    }

    public class JogRequest: Request
    {
        public JogType JogType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public JogRequest(int x, int y, JogType type) // TODO rewrite to use doubles only
        {
            Blocking = BlockingType.NonBlocking;
            JogType = type;
            X = x;
            Y = y;
        }
    }

    public class MoveToRequest: Request
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MoveToRequest(double x, double y, BlockingType blocking)
        {
            if(!(blocking == BlockingType.NonBlocking || blocking == BlockingType.ExternallyBlocking))
            {
                throw new ArgumentException("MoveToRequest can only be NonBlocking or ExternallyBlocking");
            }
            Blocking = blocking;
            X = Math.Round(x, 3); // Gcode is only good to 1um, need to compare floats with equality defined as same to 4 decimal places
            Y = Math.Round(y, 3);
        }
    }
}
