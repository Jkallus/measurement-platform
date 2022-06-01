using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Enums
{
    public enum SerialDataType
    {
        ESPFirstBootMessage,
        ESPBootloader,
        MSGINFO,
        MSGDBG,
        FNCEntryPrompt,
        UnlockPrompt,
        Status,
        RequestComplete,
        OutgoingMessage,
        RuntimeError
    }
}
