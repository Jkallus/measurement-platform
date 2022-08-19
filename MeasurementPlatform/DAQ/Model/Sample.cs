using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Model;

public struct RawSample
{
    public int XCounts { get; set; }
    public int YCounts { get; set; }
    public double Voltage { get; set; }

    public RawSample(int xcounts, int ycounts, double voltage)
    {
        XCounts = xcounts;
        YCounts = ycounts;
        Voltage = voltage;
    }
}

public struct ProcessedSample
{
    public double XCoordinate { get; set; }
    public double YCoordinate { get; set; }
    public double Z { get; set; }

    public ProcessedSample(double xcoordinate, double ycoordinate, double z)
    {
        XCoordinate = xcoordinate;
        YCoordinate = ycoordinate;
        Z = z;
    }
}

