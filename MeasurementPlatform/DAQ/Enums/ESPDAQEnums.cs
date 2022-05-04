using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Enums
{
    public enum OutgoingMessageType
    {
        Initialize = 1,
        Deinitialize = 2,
        GetVoltage = 3,
        GetEncoderCounts = 4,
        ResetEncoder = 5,
    }

    public enum IncomingMessageType
    {
        Initialize = OutgoingMessageType.Initialize,
        Deinitialize = OutgoingMessageType.Deinitialize,
        GetVoltage = OutgoingMessageType.GetVoltage,
        GetEncoderCounts = OutgoingMessageType.GetEncoderCounts,
        ResetEncoder = OutgoingMessageType.ResetEncoder
    }

    public enum ErrorCode
    {
        Success = 0,
        AlreadyInitialized = 1,
        NotCurrentlyInitialized = 2
    }

}
