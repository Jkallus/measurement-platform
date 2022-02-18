using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Model
{
    public class SerialConfig
    {
        public string COM { get; set; }
        public int BaudRate { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public int DataBits { get; set; }
        public System.IO.Ports.StopBits StopBits { get; set; }
        

        public SerialConfig()
        {
            this.COM = String.Empty;
        }
    }
}
