using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Model
{
    public class DAQStateEventArgs: EventArgs
    {
        public string State { get; set; }

        public DAQStateEventArgs(string state)
        {
            State = state;
        }
    }
}
