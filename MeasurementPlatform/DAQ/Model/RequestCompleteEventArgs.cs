using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAQ.Model
{
    public class RequestCompleteEventArgs: EventArgs
    {
        public Response Response { get; set; }

        public RequestCompleteEventArgs(Response response)
        {
            Response = response;
        }
    }
}
