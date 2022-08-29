using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Core.Models;

public class StageConfig
{
    public double XAxisLength { get; set; }
    public double YAxisLength { get; set; }

    public double XHomePosition { get; set; }
    public double YHomePosition { get; set; }

    public int MaxSpeed { get; set; }

    public override string ToString()
    {
        return $"{XAxisLength} by {YAxisLength}";
    }
}
