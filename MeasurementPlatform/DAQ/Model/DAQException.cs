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
        ErrorCode DAQError;

        public DAQException(ErrorCode error): base(error.ToFriendlyString())
        {
            DAQError = error;
        }   
    }

}
