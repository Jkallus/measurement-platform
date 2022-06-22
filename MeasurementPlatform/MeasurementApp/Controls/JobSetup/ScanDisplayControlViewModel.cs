using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.JobSetup
{
    public class ScanDisplayControlViewModel: ObservableObject
    {
        // Private member variables
        private readonly ILogger<ScanDisplayControlViewModel> _logger;
        private readonly IServiceProvider _service;
        private readonly StageConfig _stageConfig;

        // Public properties
        public PlotModel Model { get; set; }


        // Constructor
        public ScanDisplayControlViewModel(IServiceProvider service, ILogger<ScanDisplayControlViewModel> logger)
        {
            _logger = logger;
            _service = service;
            _stageConfig = _service.GetService(typeof(StageConfig)) as StageConfig;

            Model = CustomPlot();

            _logger.LogInformation("Constructed ScanDisplayControlViewModel");
        }

        // Private methods
        private PlotModel CustomPlot()
        {
            var model = new PlotModel();
            model.PlotType = PlotType.Cartesian;

            var l = new Legend
            {
                LegendSymbolLength = 30
            };
            model.Legends.Add(l);

            double edgeVisibilityOffset = 20;

            var xAxis = new LinearAxis
            {
                Minimum = 0 - edgeVisibilityOffset,
                Maximum = _stageConfig.XAxisLength + edgeVisibilityOffset,
                AbsoluteMinimum = 0 - edgeVisibilityOffset,
                AbsoluteMaximum = _stageConfig.XAxisLength + edgeVisibilityOffset,
                Position = AxisPosition.Bottom,
                MajorStep = 50,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            var yAxis = new LinearAxis
            {
                Minimum = 0 - edgeVisibilityOffset,
                Maximum = _stageConfig.YAxisLength + edgeVisibilityOffset,
                AbsoluteMinimum = 0 - edgeVisibilityOffset,
                AbsoluteMaximum = _stageConfig.YAxisLength + edgeVisibilityOffset,
                Position = AxisPosition.Left,
                MajorStep = 50,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            var edge = new RectangleAnnotation
            {
                MinimumX = 0,
                MaximumX = _stageConfig.XAxisLength,
                MinimumY = 0,
                MaximumY = _stageConfig.YAxisLength,
                Fill = OxyColors.Transparent,
                Stroke = OxyColors.Black,
                StrokeThickness = 2
            };
            model.Annotations.Add(edge);

            //var scanPoints = new ScatterSeries
            //{
            //    Title = "Scan Points",
            //    MarkerType = MarkerType.Circle,
            //    MarkerFill = OxyColors.Red,
            //    MarkerSize = 3,
            //};
            //model.Series.Add(scanPoints);

            double centerX = _stageConfig.XAxisLength / 2;
            double centerY = _stageConfig.YAxisLength / 2;
            int defaultBoxSize = 50;
            var samplingBox = new RectangleAnnotation
            {

                MinimumX = centerX - defaultBoxSize,
                MaximumX = centerX + defaultBoxSize,
                MinimumY = centerY - defaultBoxSize,
                MaximumY = centerY + defaultBoxSize,
                Fill = OxyColors.Transparent,
                Stroke = OxyColors.Blue,
                StrokeThickness = 2
            };
            model.Annotations.Add(samplingBox);

            var dragPoints = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Green,
                MarkerSize = 6
            };
            ScatterPoint bottomLeft = new(samplingBox.MinimumX, samplingBox.MinimumY);
            ScatterPoint topLeft = new(samplingBox.MinimumX, samplingBox.MaximumY);
            ScatterPoint topRight = new(samplingBox.MaximumX, samplingBox.MaximumY);
            ScatterPoint bottomRight = new(samplingBox.MaximumX, samplingBox.MinimumY);
            dragPoints.Points.Add(bottomLeft);
            dragPoints.Points.Add(topLeft);
            dragPoints.Points.Add(topRight);
            dragPoints.Points.Add(bottomRight);
            const int bottomLeftIdx = 0;
            const int topLeftIdx = 1;
            const int topRightIdx = 2;
            const int bottomRightIdx = 3;

            int indexOfPointToMove = -1;

            dragPoints.MouseDown += (s, e) =>
            {
                if(e.ChangedButton == OxyMouseButton.Left)
                {
                    int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
                    var nearestPoint = dragPoints.Transform(dragPoints.Points[indexOfNearestPoint].X, dragPoints.Points[indexOfNearestPoint].Y);

                    if((nearestPoint - e.Position).Length < 10)
                    {
                        indexOfPointToMove = indexOfNearestPoint;
                    }

                    e.Handled = true;
                }
            };

            dragPoints.MouseMove += (s, e) =>
            {
                if(indexOfPointToMove >= 0)
                {
                    var newLocation = dragPoints.InverseTransform(e.Position);
                    dragPoints.Points[indexOfPointToMove] = new ScatterPoint(newLocation.X, newLocation.Y);
                    ScatterPoint movePoint;
                    switch (indexOfPointToMove) 
                    {
                        case bottomLeftIdx:
                            samplingBox.MinimumX = newLocation.X;
                            samplingBox.MinimumY = newLocation.Y;
                            movePoint = dragPoints.Points[topLeftIdx];
                            dragPoints.Points[topLeftIdx] = new ScatterPoint(newLocation.X, movePoint.Y);
                            movePoint = dragPoints.Points[bottomRightIdx];
                            dragPoints.Points[bottomRightIdx] = new ScatterPoint(movePoint.X, newLocation.Y);
                            break;
                        case topLeftIdx:
                            samplingBox.MinimumX = newLocation.X;
                            samplingBox.MaximumY = newLocation.Y;
                            movePoint = dragPoints.Points[bottomLeftIdx];
                            dragPoints.Points[bottomLeftIdx] = new ScatterPoint(newLocation.X, movePoint.Y);
                            movePoint = dragPoints.Points[topRightIdx];
                            dragPoints.Points[topRightIdx] = new ScatterPoint(movePoint.X, newLocation.Y);
                            break;
                        case topRightIdx:
                            samplingBox.MaximumX = newLocation.X;
                            samplingBox.MaximumY = newLocation.Y;
                            movePoint = dragPoints.Points[topLeftIdx];
                            dragPoints.Points[topLeftIdx] = new ScatterPoint(movePoint.X, newLocation.Y);
                            movePoint = dragPoints.Points[bottomRightIdx];
                            dragPoints.Points[bottomRightIdx] = new ScatterPoint(newLocation.X, movePoint.Y);
                            break;
                        case bottomRightIdx:
                            samplingBox.MaximumX = newLocation.X;
                            samplingBox.MinimumY = newLocation.Y;
                            movePoint = dragPoints.Points[topRightIdx];
                            dragPoints.Points[topRightIdx] = new ScatterPoint(newLocation.X, movePoint.Y);
                            movePoint = dragPoints.Points[bottomLeftIdx];
                            dragPoints.Points[bottomLeftIdx] = new ScatterPoint(movePoint.X, newLocation.Y);
                            break;
                        default:
                            throw new IndexOutOfRangeException("Invalid point index");
                    }

                    Model.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

            dragPoints.MouseUp += (s, e) =>
            {
                indexOfPointToMove = -1;
                var point = dragPoints.InverseTransform(e.Position);
                model.InvalidatePlot(false);
                e.Handled = true;
            };

            model.Series.Add(dragPoints);

            return model;
        }
    }
}
