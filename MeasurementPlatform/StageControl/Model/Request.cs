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
        public double Xcoordinate { get; set; }
        public double Ycoordinate { get; set; }

        public JogRequest(int xcoordinate, int ycoordinate)
        {
            Xcoordinate = xcoordinate; 
            Ycoordinate = ycoordinate;
        }
    }
}
