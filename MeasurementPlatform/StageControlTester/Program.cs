using System;
using System.IO.Ports;
using StageControl;

namespace StageControlTester
{
    internal class Program
    {    
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