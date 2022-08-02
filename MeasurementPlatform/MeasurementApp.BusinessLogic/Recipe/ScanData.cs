using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Recipe;

public struct Sample
{
    public int Xcounts { get; set; }
    public int Ycounts { get; set; }
    public double Volts { get; set; }

    public double ScaledValue
    {
        get => (1.0f / (0.023f * Volts + 0.0046f)) - 8;
    }

    public Sample(int xcounts, int ycounts, double volts)
    {
        Xcounts = xcounts;
        Ycounts = ycounts;
        Volts = volts;
    }
}

public class ScanData
{
    private List<Sample> _resultData;
    public List<Sample> ResultData
    {
        get => _resultData;
        set => _resultData = value;
    }

    public int XAxisSampleCount { get; set; }
    public int YAxisSampleCount { get; set; }

    public ScanData(int xcount, int ycount)
    {
        _resultData = new List<Sample>(xcount * ycount);
        XAxisSampleCount = xcount;
        YAxisSampleCount = ycount;
    }
}
