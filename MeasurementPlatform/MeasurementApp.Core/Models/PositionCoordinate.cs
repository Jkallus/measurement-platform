using System;
using System.Collections.Generic;
using System.Text;

namespace MeasurementApp.Core.Models;

public class PositionCoordinate
{
    public double X { get; set; }
    public double Y { get; set; }

    public PositionCoordinate(double x, double y)
    {
        X = x;
        Y = y;
    }

    public (double x, double y) AsTuple()
    {
        return (X, Y);
    }

    public override string ToString()
    {
        return $"({X.ToString("0.000")}, {Y.ToString("0.000")})";
    }

    public PositionCoordinate Clone()
    {
        return this.MemberwiseClone() as PositionCoordinate;
    }
}
