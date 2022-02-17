using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Events
{
    public class StatusUpdateEventArgs
    {
        public SerialDataItem Update { get; set; }

        public StatusUpdateEventArgs(SerialDataItem update)
        {
            Update = update;
        }

    }

}
