using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Enums
{
    public static class ErrorCodeExtensions
    {
        public static string ToFriendlyString(this ErrorCode error)
        {
            switch (error)
            {
                case ErrorCode.Success:
                    return "Successful";
                case ErrorCode.AlreadyInitialized:
                    return "DAQ is already initialized";
                case ErrorCode.NotCurrentlyInitialized:
                    return "DAQ is not initialized yet";
                default:
                    return "Unknown DAQ error";

            }

        }
    }

    public enum MessageType
    {
        Initialize = 1,
        Deinitialize = 2,
        GetVoltage = 3,
        GetEncoderCounts = 4,
        ResetEncoder = 5,

        UnexpectedMessage = 101
    }


    //public enum OutgoingMessageType
    //{
    //    Initialize = 1,
    //    Deinitialize = 2,
    //    GetVoltage = 3,
    //    GetEncoderCounts = 4,
    //    ResetEncoder = 5,
    //}

    //public enum IncomingMessageType
    //{
    //    Initialize = OutgoingMessageType.Initialize,
    //    Deinitialize = OutgoingMessageType.Deinitialize,
    //    GetVoltage = OutgoingMessageType.GetVoltage,
    //    GetEncoderCounts = OutgoingMessageType.GetEncoderCounts,
    //    ResetEncoder = OutgoingMessageType.ResetEncoder
    //}

    public enum ErrorCode
    {
        Success = 0,
        AlreadyInitialized = 1,
        NotCurrentlyInitialized = 2
    }

}
