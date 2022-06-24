using MeasurementApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Recipe
{
    public class ScanRecipe
    {
        public PositionCoordinate BottomLeft { get; set; }
        public PositionCoordinate TopLeft { get; set; }
        public PositionCoordinate TopRight { get; set; }
        public PositionCoordinate BottomRight { get; set; }
        public ScanDimension ScanPitch { get; set; }
        public ScanDimension XDimension
        {
            get => new ScanDimension(BottomRight.X - BottomLeft.X, Units.Millimeters);
        }
        public ScanDimension YDimension
        {
            get => new ScanDimension(TopLeft.Y - BottomLeft.Y, Units.Millimeters);
        }
        public ScanDimension ScanArea
        {
            get => new ScanDimension(XDimension.Value * YDimension.Value, Units.SquareMillimeters);
        }

        public ScanRecipe(PositionCoordinate bottomLeft, PositionCoordinate topLeft, PositionCoordinate topRight, PositionCoordinate bottomRIght, ScanDimension samplePitch)
        {
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRIght;
            ScanPitch = samplePitch;
        }

        public ScanRecipe()
        {
            BottomLeft = new(0, 0);
            TopLeft = new(0, 0);
            TopRight = new(0, 0);
            BottomRight = new(0, 0);
            ScanPitch = new(1, Units.Millimeters);
        }

        public List<(double x, double y)> GetScanPoints()
        {
            List<(double x, double y)> points = new();

            int xSampleCount = (int)(XDimension.Value / ScanPitch.Value);
            int ySampleCount = (int)(YDimension.Value / ScanPitch.Value);

            double xPoint = BottomLeft.X;
            double yPoint = BottomLeft.Y;

            for (int ySamples = 0; ySamples < ySampleCount; ySamples++)
            {
                for (int xSamples = 0; xSamples < xSampleCount; xSamples++)
                {
                    points.Add((xPoint + (xSamples * ScanPitch.Value), yPoint + (ySamples * ScanPitch.Value)));
                }
            }

            return points;
            

        }
    }
}
