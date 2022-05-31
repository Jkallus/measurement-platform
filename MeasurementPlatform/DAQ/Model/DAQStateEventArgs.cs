using DAQ.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Model
{
    public class DAQStateEventArgs: EventArgs
    {
        public DAQState State { get; set; }

        public DAQStateEventArgs(DAQState state)
        {
            State = state;
        }

        public string ToFriendlyString()
        {
            switch (State)
            {
                case DAQState.Uninitialized:
                    return "DAQ Uninitialized";
                case DAQState.Initializing:
                    return "DAQ Initializing";
                case DAQState.Initialized:
                    return "DAQ Initialized";
                default:
                    throw new Exception("Invalid DAQState value");
            }
        }
    }


}
