using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Enums
{
    public enum LifetimeFNCState
    {
        Unknown,
        FirstBoot,
        SecondBoot,
        FNCInitStart,
        FNCInitFinish,
        FNCReady
    }
}
