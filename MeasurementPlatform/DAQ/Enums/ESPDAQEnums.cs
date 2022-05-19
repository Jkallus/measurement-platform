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
                case ErrorCode.ADCInitFail:
                    return "DAQ ADC module failed to initialize";
                case ErrorCode.EncoderInitFail:
                    return "DAQ Encoder module failed to initialize";
                case ErrorCode.DAQInitFail:
                    return "DAQ ADC and Encoder failed to initialize";
                case ErrorCode.DAQDeinitFail:
                    return "DAQ unable to deinitialize";
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

    public enum ErrorCode
    {
        Success = 0,
        AlreadyInitialized = 1,
        NotCurrentlyInitialized = 2,
        ADCInitFail = 3,
        EncoderInitFail = 4,
        DAQInitFail = 5,
        DAQDeinitFail = 6,
    }

}
