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
                case ErrorCode.GetEncoderCountFail:
                    return "DAQ failed to read encoder count";
                case ErrorCode.GetVoltageFail:
                    return "DAQ failed to read voltage";
                case ErrorCode.ResetEncoderFail:
                    return "DAQ failed to reset encoder counts";
                case ErrorCode.StartStreamFail:
                    return "DAQ failed to initiate stream";
                case ErrorCode.StopStreamFail:
                    return "DAQ failed to stop stream";
                case ErrorCode.InvalidParameter:
                    return "DAQ received invalid parameter";
                case ErrorCode.MissingCommand:
                    return "DAQ message is missing command";
                case ErrorCode.CurrentlyStreaming:
                    return "DAQ is currently in streaming mode";
                default:
                    return "Unknown DAQ error";

            }

        }
    }

    public enum MessageType
    {
        Initialize =        1,
        Deinitialize =      2,
        GetVoltage =        3,
        GetEncoderCounts =  4,
        ResetEncoder =      5,
        StartStream =       6,
        StopStream =        7,
        StreamData =        61,
        UnexpectedMessage = 101
    }

    public enum DAQState
    {
        Uninitialized,
        Initializing,
        Initialized,
        Streaming
    }

    public enum ErrorCode
    {
        Success =                 0,
        AlreadyInitialized =      1,
        NotCurrentlyInitialized = 2,
        ADCInitFail =             3,
        EncoderInitFail =         4,
        DAQInitFail =             5,
        DAQDeinitFail =           6,
        GetEncoderCountFail =     7,
        GetVoltageFail =          8,
        ResetEncoderFail =        9,
        StartStreamFail =         10,
        StopStreamFail =          11,
        InvalidParameter =        12,
        MissingCommand =          13,
        CurrentlyStreaming =      14,
    }

}
