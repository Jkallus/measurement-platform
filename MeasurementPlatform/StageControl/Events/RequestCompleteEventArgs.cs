using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Events
{
    public class RequestCompleteEventArgs: EventArgs
    {
        public Request Req;

        public RequestCompleteEventArgs(Request req)
        {
            Req = req;
        }
    }
}
