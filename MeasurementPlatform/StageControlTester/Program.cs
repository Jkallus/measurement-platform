using System;
using System.IO.Ports;
using StageControl;
using StageControl.Enums;
using StageControl.Model;

namespace StageControlTester
{
    internal class Program
    {

        //static FluidNCController? FNC;

        //static void StateChanged(object? sender, FNCStateChangedEventArgs e)
        //{
        //    Console.WriteLine(e.State.ToString());
        //    if(e.State == LifetimeFNCState.FNCReady && FNC != null)
        //    {
        //        FNC.RequestStatus();
        //    }
        //}

        static async Task Main(string[] args)
        {
            FNCMachineControl machine = new FNCMachineControl();
            await machine.Initialize();

            
            await machine.Home(HomingAxes.X);
            Console.WriteLine("Homed X");

            await machine.Home(HomingAxes.Y);
            Console.WriteLine("Homed Y");

            Console.WriteLine("Machine is Homed!");

            while (true) 
            {
                Console.ReadKey();
            }

            //FNC = new FluidNCController();
            //FNC.FNCStateChanged += StateChanged;
            //FNC.Connect();

            //while(true)
            //{

            //}
        }
    }
}