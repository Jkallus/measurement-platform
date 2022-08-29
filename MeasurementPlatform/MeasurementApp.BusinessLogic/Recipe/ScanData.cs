using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Model;

namespace MeasurementApp.BusinessLogic.Recipe;

public class ScanData
{
    private List<ProcessedSample> _data;
    public List<ProcessedSample> Data
    {
        get => _data;
        set => _data = value;
    }

    public ScanData()
    {
        _data = new List<ProcessedSample>();
    }
}
