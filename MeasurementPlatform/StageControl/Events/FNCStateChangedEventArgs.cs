using StageControl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Events
{
    public class FNCStateChangedEventArgs : EventArgs
    {
        public LifetimeFNCState State { get; set; }

        public FNCStateChangedEventArgs(LifetimeFNCState state)
        {
            this.State = state;
        }

        public FNCStateChangedEventArgs()
        {

        }
    }
}
