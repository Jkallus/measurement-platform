using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;

namespace DAQ.Model
{
    public class Command
    {
        public OutgoingMessageType MessageType { get; set; }

        public Command(OutgoingMessageType messageType)
        {
            MessageType = messageType;
        }

        public override string ToString() => ((int)MessageType).ToString() + ";";
        
    }
}
