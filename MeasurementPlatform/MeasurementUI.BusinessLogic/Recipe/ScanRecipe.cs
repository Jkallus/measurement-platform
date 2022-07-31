using MeasurementApp.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Recipe
{
    public class ScanRecipe
    {
        public string Name { get; set; }
        public PositionCoordinate BottomLeft { get; set; }
        public PositionCoordinate TopLeft { get; set; }
        public PositionCoordinate TopRight { get; set; }
        public PositionCoordinate BottomRight { get; set; }
        public ScanDimension ScanPitch { get; set; }

        [JsonIgnore]
        public ScanDimension XDimension
        {
            get => new ScanDimension(BottomRight.X - BottomLeft.X, Units.Millimeters);
        }
        [JsonIgnore]
        public ScanDimension YDimension
        {
            get => new ScanDimension(TopLeft.Y - BottomLeft.Y, Units.Millimeters);
        }
        [JsonIgnore]
        public ScanDimension ScanArea
        {
            get => new ScanDimension(XDimension.Value * YDimension.Value, Units.SquareMillimeters);
        }

        [JsonIgnore]
        public int XSamples => (int)(XDimension.Value / (ScanPitch.Value / 1000.0));

        [JsonIgnore]
        public int YSamples => (int)(YDimension.Value / (ScanPitch.Value / 1000.0));


        public ScanRecipe(string name, PositionCoordinate bottomLeft, PositionCoordinate topLeft, PositionCoordinate topRight, PositionCoordinate bottomRIght, ScanDimension scanPitch)
        {
            Name = name;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRIght;
            ScanPitch = scanPitch;
        }

        public ScanRecipe()
        {
            Name = $"ScanRecipe_{DateTime.Now.ToString("yyyyMMddTHHmmss")}";
            BottomLeft = new(0, 0);
            TopLeft = new(0, 0);
            TopRight = new(0, 0);
            BottomRight = new(0, 0);
            ScanPitch = new(1, Units.Millimeters);
        }

        public List<(double x, double y)> GetScanPoints()
        {
            List<(double x, double y)> points = new();

            int xSampleCount = (int)(XDimension.Value / (ScanPitch.Value / 1000.0));
            int ySampleCount = (int)(YDimension.Value / (ScanPitch.Value / 1000.0));

            double xMin = BottomLeft.X;
            double yMin = BottomLeft.Y;
            double xMax = BottomRight.X;

            bool leftToRight = true;

            for (int ySamples = 0; ySamples < ySampleCount; ySamples++)
            {
                if(leftToRight)
                {
                    for (int xSamples = 0; xSamples < xSampleCount; xSamples++)
                    {
                        points.Add((xMin + (xSamples * (ScanPitch.Value / 1000.0)), yMin + (ySamples * (ScanPitch.Value / 1000.0))));
                    }
                }
                else
                {
                    for(int xSamples = 0; xSamples < xSampleCount; xSamples++)
                    {
                        points.Add((xMax - (xSamples * (ScanPitch.Value / 1000.0)), yMin + (ySamples * (ScanPitch.Value / 1000.0))));
                    }
                }
                leftToRight = !leftToRight; // flip direction each row
            }

            return points;
            

        }
    }
}
