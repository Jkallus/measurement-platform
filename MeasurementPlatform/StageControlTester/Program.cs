using System;
using System.IO.Ports;
using StageControl;
using StageControl.Enums;
using StageControl.Model;
using MeasurementUI.BusinessLogic.Configuration;

namespace StageControlTester
{
    internal class Program
    {
        //    static async Task Main(string[] args)
        //    {
        //        FNCMachineControl machine = new FNCMachineControl();
        //        await machine.Initialize();


        //        await machine.Home(HomingAxes.X);
        //        Console.WriteLine("Homed X");

        //        await machine.Home(HomingAxes.Y);
        //        Console.WriteLine("Homed Y");

        //        Console.WriteLine("Machine is Homed!");

        //        while (true) 
        //        {
        //            Console.ReadKey();
        //        }
        //    }
        //}

        public static void Main(string[] args)
        {
            MachineConfiguration machineConfiguration = new MachineConfiguration();
            machineConfiguration.SerialConfig.BaudRate = 115200;
            machineConfiguration.SerialConfig.DataBits = 8;
            machineConfiguration.SerialConfig.StopBits = StopBits.One;
            machineConfiguration.SerialConfig.Parity = Parity.None;
            machineConfiguration.SerialConfig.COM = "COM3";

            machineConfiguration.StageConfig.XAxisLength = 100.0;
            machineConfiguration.StageConfig.YAxisLength = 100.0;

            machineConfiguration.SaveToJSON(Directory.GetCurrentDirectory() + "//machineconfig.json");

        }
    }
}