using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;

namespace DAQ.Model
{
    public class DAQException: Exception
    {
        public ErrorCode DAQError { get; set; }

        public DAQException(ErrorCode error): base(error.ToFriendlyString())
        {
            DAQError = error;
        }   
    }

}
