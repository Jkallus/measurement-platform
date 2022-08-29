using MeasurementApp.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Recipe;

public class ScanRecipe
{
    public string Name { get; set; }
    public PositionCoordinate BottomLeft { get; set; }
    public PositionCoordinate TopLeft { get; set; }
    public PositionCoordinate TopRight { get; set; }
    public PositionCoordinate BottomRight { get; set; }
    public int SamplingRate { get; set; }

    public ScanDimension StaticAxisStepSize { get; set; }

    [JsonIgnore]
    public ScanDimension XDimension => new ScanDimension(BottomRight.X - BottomLeft.X, Units.Millimeters);
    [JsonIgnore]
    public ScanDimension YDimension => new ScanDimension(TopLeft.Y - BottomLeft.Y, Units.Millimeters);
    [JsonIgnore]
    public ScanDimension ScanArea => new ScanDimension(XDimension.Value * YDimension.Value, Units.SquareMillimeters);

    [JsonIgnore]
    public PositionCoordinate ScanStartLocation
    {
        get => BottomRight;
    }

    public ScanRecipe(string name, PositionCoordinate bottomLeft, PositionCoordinate topLeft, PositionCoordinate topRight, PositionCoordinate bottomRight, int samplingRate, ScanDimension staticAxisStepSize)
    {
        Name = name;
        BottomLeft = bottomLeft;
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        SamplingRate = samplingRate;
        StaticAxisStepSize = staticAxisStepSize;
    }

    public ScanRecipe()
    {
        Name = $"ScanRecipe_{DateTime.Now.ToString("yyyyMMddTHHmmss")}";
        BottomLeft = new(0, 0);
        TopLeft = new(0, 0);
        TopRight = new(0, 0);
        BottomRight = new(0, 0);
        SamplingRate = 0;
        StaticAxisStepSize = new ScanDimension(0.0, Units.Millimeters);
    }

    public List<PositionCoordinate> GetPositions()
    {
        List<PositionCoordinate> positions = new List<PositionCoordinate>();

        PositionCoordinate workingPosition = ScanStartLocation.Clone();

        bool onRightSide = true;

        while(workingPosition.Y < TopRight.Y)
        {
            positions.Add(workingPosition.Clone());
            if (onRightSide)
            {
                workingPosition.X -= XDimension.Value;
                onRightSide = false;
            }
            else
            {
                workingPosition.X += XDimension.Value;
                onRightSide = true;
            }
            positions.Add(workingPosition.Clone());
            workingPosition.Y += StaticAxisStepSize.Value;
        }

        workingPosition.Y = TopRight.Y;
        positions.Add(workingPosition.Clone());

        workingPosition.X += onRightSide ? (-XDimension.Value) : XDimension.Value;
        positions.Add(workingPosition.Clone());

        return positions;
    }
}
