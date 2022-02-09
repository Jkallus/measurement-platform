using System;
using System.IO.Ports;
using StageControl;
using StageControl.Enums;

namespace StageControlTester
{
    internal class Program
    {

        static FluidNCController? FNC;

        static void StateChanged(object? sender, FNCStateChangedEventArgs e)
        {
            Console.WriteLine(e.State.ToString());
            if(e.State == LifetimeFNCState.FNCReady && FNC != null)
            {
                FNC.RequestStatus();
            }
        }

        static void Main(string[] args)
        {
 

            FNC = new FluidNCController();
            FNC.FNCStateChanged += StateChanged;
            FNC.Connect();

            while(true)
            {

            }
        }
    }
}