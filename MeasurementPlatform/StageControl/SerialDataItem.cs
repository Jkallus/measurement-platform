using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl
{
    public class SerialDataItem
    {
        public string Data { get; set; }
        public DateTime Time { get; set; }
        public SerialDataType Type { get; set; }

        public SerialDataItem(string data, DateTime time, SerialDataType type)
        {
            this.Data = data;
            this.Time = time;
            this.Type = type;
        }

        public SerialDataItem()
        {
            
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}]: '{2}'", Time.ToString(), Type.ToString(), Data);
        }


    }
}
