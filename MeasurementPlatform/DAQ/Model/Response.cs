using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;

namespace DAQ.Model
{
    public class Response
    {
        public IncomingMessageType MessageType { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public Response(IncomingMessageType messageType, ErrorCode errorCode)
        {
            this.MessageType = messageType;
            this.ErrorCode = errorCode;
        }
    }

    public class MeasureVoltageResponse: Response
    {
        public float Voltage { get; set; }

        public MeasureVoltageResponse(IncomingMessageType messageType, ErrorCode errorCode, float voltage): base(messageType, errorCode)
        {
            Voltage = voltage;
        }
    }

    public class MeasureEncoderCountsResponse: Response
    {
        public int Counts { get; set; }

        public MeasureEncoderCountsResponse(IncomingMessageType messageType, ErrorCode errorCode, int counts): base(messageType, errorCode)
        {
            Counts = counts;
        }
    }
}
