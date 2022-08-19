using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Model;

namespace DAQ.Interfaces;
public interface ISampleProcessor
{
    public (int XOriginCount, int YOriginCount) ScanEncoderOrigin { get; set; } // Coordinates of first scan point in encoder counts
    public ProcessedSample ProcessSample(RawSample sample);
}
