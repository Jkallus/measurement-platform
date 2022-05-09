using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Core.Models
{
    public class SerialConfig
    {
        public string COM { get; set; }
        public int BaudRate { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public int DataBits { get; set; }
        public System.IO.Ports.StopBits StopBits { get; set; }
        

        public SerialConfig(string com)
        {
            COM = com;
            BaudRate = 115200;
            Parity = System.IO.Ports.Parity.None;
            DataBits = 8;
            StopBits = System.IO.Ports.StopBits.One;
        }

        public SerialConfig()
        {
            this.COM = String.Empty;
        }
    }
}
