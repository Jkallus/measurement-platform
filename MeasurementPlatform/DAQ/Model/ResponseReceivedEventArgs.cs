using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAQ.Model
{
    public class ResponseReceivedEventArgs: EventArgs
    {
        public Response Response { get; set; }

        public ResponseReceivedEventArgs(Response response)
        {
            Response = response;
        }
    }
}
