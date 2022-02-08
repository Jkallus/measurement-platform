using System;
using System.IO.Ports;
using StageControl;

namespace StageControlTester
{
    internal class Program
    {
        //    static SerialPort serialPort;

        //    static void Main(string[] args)
        //    {
        //        Console.WriteLine("Hello World!");

        //        Thread readThread = new Thread(Read);

        //        serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);

        //        serialPort.Open();

        //        readThread.Start();


        //        while (true)
        //        {

        //        }

        //    }

        //    public static void Read()
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                string message = serialPort.ReadLine();
        //                Console.WriteLine(message);
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(e.ToString());
        //            }
        //        }
        //    }
        //}
    
        static void DataReceived(object sender, SerialDataItemReceivedEventArgs e)
        {
            Console.WriteLine(e.item.ToString());
        }


        static void Main(string[] args)
        {
            SerialController serialController = new SerialController();

            serialController.SerialDataItemReceived += DataReceived;

            while(true)
            {

            }
        }
    }
}