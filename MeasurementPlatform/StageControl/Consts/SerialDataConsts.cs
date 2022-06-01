using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageControl.Consts
{
    public static class SerialDataConsts
    {
        public const string ESPFirstBootMessageMarker = "ets ";
        public const string ESPBootloaderMessageMarker = "rst:0x1 (POWERON_RESET),boot:0x13 (SPI_FAST_FLASH_BOOT)";
        public const string DoubleLineBreak = "\r\n\r\n";
        public const string LineBreak = "\r\n";
        public const string MSGINFOMessageMarker = "[MSG:INFO: ";
        public const string MSGDBGMessageMarker = "[MSG:DBG: ";
        public const string FNCEntryPromptMessageMarker = "Grbl 3.3 [FluidNC v3.3.1 (wifi) '$' for help]";
        public const string MSGEndMarker = "]";
        public const string StatusStartMessageMarker = "<";
        public const string StatusEndMessageMarker = ">";
        public const string RequestCompleteMessageMarker = "ok";
        public const string RuntimeErrorMessageMarker = "error:";
        public const string Newline = "\n";
    }
}
