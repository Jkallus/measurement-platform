using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Model;

namespace DAQ.Interfaces;
public interface ISampleProcessor
{
    public ProcessedSample ProcessSample(RawSample sample);
}
