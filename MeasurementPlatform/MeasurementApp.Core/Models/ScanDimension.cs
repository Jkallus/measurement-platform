using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeasurementApp.Core.Models;

public enum Units 
{
    Millimeters,
    Micrometers,
    SquareMillimeters,
    SquareMicrometers,
}

public static class UnitsExtensions
{
    public static string ToSuperScript(this int number)
    {
        if (number == 0 ||
            number == 1)
            return "";

        const string SuperscriptDigits =
            "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

        string Superscript = "";

        if (number < 0)
        {
            //Adds superscript minus
            Superscript = ((char)0x207B).ToString();
            number *= -1;
        }


        Superscript += new string(number.ToString()
                                        .Select(x => SuperscriptDigits[x - '0'])
                                        .ToArray()
                                  );

        return Superscript;
    }

    public static string ToFriendlyString(this Units unit, bool shorthand = true)
    {
        switch(unit)
        {
            case Units.Millimeters:
                return shorthand ? "mm" : "millimeters";
            case Units.Micrometers:
                return shorthand ? "um" : "microns";
            case Units.SquareMillimeters:
                return shorthand ? $"mm{2.ToSuperScript()}" : $"millimeters{2.ToSuperScript()}";
            case Units.SquareMicrometers:
                return shorthand ? $"um{2.ToSuperScript()}" : $"microns{2.ToSuperScript()}";
            default:
                throw new ArgumentException("Invalid units argument");
        }
    }
}

public class ScanDimension
{
    public double Value { get; set; }
    public Units Unit { get; set; }

    public ScanDimension(double value, Units unit)
    {
        Value = value;
        Unit = unit;
    }

    public override string ToString()
    {
        return $"{Value.ToString("0.000")} {Unit.ToFriendlyString()}";
    }
}