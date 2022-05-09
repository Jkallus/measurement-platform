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
        public MessageType MessageType { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public Response(MessageType messageType, ErrorCode errorCode)
        {
            this.MessageType = messageType;
            this.ErrorCode = errorCode;
        }
    }

    
    public class DataResponse<T>: Response
    {
        public T Data { get; set; }

        public DataResponse(MessageType messageType, ErrorCode errorCode, T data): base(messageType, errorCode)
        {
            Data = data;
        }
    }

    public class MeasureVoltageResponse: Response
    {
        public float Voltage { get; set; }

        public MeasureVoltageResponse(MessageType messageType, ErrorCode errorCode, float voltage): base(messageType, errorCode)
        {
            Voltage = voltage;
        }
    }

    public class MeasureEncoderCountsResponse: Response
    {
        public int Counts { get; set; }

        public MeasureEncoderCountsResponse(MessageType messageType, ErrorCode errorCode, int counts): base(messageType, errorCode)
        {
            Counts = counts;
        }
    }
}
