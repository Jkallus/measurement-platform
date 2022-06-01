using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Events
{
    public class RuntimeErrorEventArgs: EventArgs
    {
        public int Error { get; set; }
        public string Message { get; set; }

        public RuntimeErrorEventArgs(string message, int error)
        {
            Error = error;
            Message = message;
        }

    }
}
