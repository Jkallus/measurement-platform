using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public abstract class Request
    {
        
    }

    public class HomingRequest: Request
    {
        public HomingAxes Axes { get; set; }   

        public HomingRequest(HomingAxes axes)
        {
            this.Axes = axes;   
        }
    }

    public class JogRequest: Request
    {
        public JogType JogType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public JogRequest(int x, int y, JogType type)
        {
            JogType = type;
            X = x;
            Y = y;
        }
    }
}
