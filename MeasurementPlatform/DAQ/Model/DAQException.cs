using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Enums;
using System.Runtime.CompilerServices;

namespace DAQ.Model
{
    public class DAQException: Exception
    {
        public ErrorCode DAQError { get; set; }
        public string Caller { get; set; }
        public int LineNumber { get; set; }

        public DAQException(ErrorCode error, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0) : base(error.ToFriendlyString())
        {
            DAQError = error;
            Caller = caller;
            LineNumber = lineNumber;
        }
    }

}
