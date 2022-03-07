using System;
using System.IO.Ports;
using StageControl;
using StageControl.Enums;
using StageControl.Model;
using MeasurementUI.BusinessLogic.Configuration;
using StageControl.Events;
//using STILAdapter;

namespace StageControlTester
{
    //internal class Program
    //{
    //    static ManualResetEvent _quitEvent = new ManualResetEvent(false);

    //    static void MotionController_PositionChanged(object? sender, PositionChangedEventArgs e)
    //    {
    //        Console.WriteLine(String.Format("X: {0}, Y: {0}", e.X, e.Y));
    //    }



    //    static async Task Main(string[] args)
    //    {
    //        Console.CancelKeyPress += (sender, e) =>
    //        {
    //            _quitEvent.Set();
    //            e.Cancel = true;
    //        };

    //        MachineConfiguration machineConfiguration = new MachineConfiguration();
    //        machineConfiguration.SerialConfig.BaudRate = 115200;
    //        machineConfiguration.SerialConfig.DataBits = 8;
    //        machineConfiguration.SerialConfig.StopBits = StopBits.One;
    //        machineConfiguration.SerialConfig.Parity = Parity.None;
    //        machineConfiguration.SerialConfig.COM = "COM3";

    //        machineConfiguration.StageConfig.XAxisLength = 100.0;
    //        machineConfiguration.StageConfig.YAxisLength = 100.0;



    //        FNCMachineControl machine = new FNCMachineControl(machineConfiguration.SerialConfig, machineConfiguration.StageConfig);
    //        machine.PositionChanged += MotionController_PositionChanged;
    //        await machine.Initialize();
    //        await machine.Home(HomingAxes.X);
    //        Console.WriteLine("Homed X");
    //        await machine.Home(HomingAxes.Y);
    //        Console.WriteLine("Homed Y");
    //        Console.WriteLine("Machine is Homed!");

    //        _quitEvent.WaitOne();

    //        Console.WriteLine("Exiting");
    //    }
    //}

    public class Program
    {
        static void Main(string[] args)
        {
            //SerialPort serialPort = new SerialPort();
            //serialPort.PortName = "COM3";
            //serialPort.BaudRate = 115200;
            //serialPort.DataBits = 8;
            //serialPort.StopBits = StopBits.One;
            //serialPort.Parity = Parity.None;

            //serialPort.RtsEnable = false;
            //serialPort.Open();

            //serialPort.RtsEnable = true;
            //Thread.Sleep(10);
            //serialPort.RtsEnable = false;

            ////serialPort.RtsEnable = false;
            ////serialPort.DtrEnable = true;
            ////Thread.Sleep(10);
            ////serialPort.RtsEnable = true;
            ////serialPort.DtrEnable = false;
            ////Thread.Sleep(10);
            ////serialPort.RtsEnable = true;
            ////serialPort.DtrEnable = true;
            ////Thread.Sleep(10);

            //serialPort.Close();

            //Runner.DoWork();
            
        }
    }
}