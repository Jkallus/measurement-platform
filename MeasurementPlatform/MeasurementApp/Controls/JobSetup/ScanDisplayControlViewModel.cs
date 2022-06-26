using MeasurementApp.Controls.RecipeManagement;
using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using StageControl.Model;
using System;
using MeasurementApp.Core.Oxyplot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.RecipeSetup
{
    public class ScanDisplayControlViewModel: ObservableObject
    {
        // Private member variables
        private readonly ILogger<ScanDisplayControlViewModel> _logger;
        private readonly IServiceProvider _service;
        private readonly StageConfig _stageConfig;

        private const int bottomLeftIdx = 0;
        private const int topLeftIdx = 1;
        private const int topRightIdx = 2;
        private const int bottomRightIdx = 3;

        ScatterSeries dragPoints;
        RectangleAnnotation samplingBox;

        // Public properties
        
        public ViewResolvingPlotModel Model { get; set; }

        PositionCoordinate BottomLeft { get; set; }
        PositionCoordinate TopLeft { get; set; }
        PositionCoordinate TopRight { get; set; }
        PositionCoordinate BottomRight { get; set; }

        // Constructor
        public ScanDisplayControlViewModel(IServiceProvider service, ILogger<ScanDisplayControlViewModel> logger)
        {
            _logger = logger;
            _service = service;
            _stageConfig = _service.GetService(typeof(StageConfig)) as StageConfig;

            Model = CustomPlot();
            SendUpdate();
            _logger.LogInformation("Constructed ScanDisplayControlViewModel");
            WeakReferenceMessenger.Default.Register<EditRecipeMessage>(this, (r, m) =>
            {
                BottomLeft = m.Recipe.BottomLeft;
                TopLeft = m.Recipe.TopLeft;
                TopRight = m.Recipe.TopRight;
                BottomRight = m.Recipe.BottomRight;
                dragPoints.Points[bottomLeftIdx] = new ScatterPoint(BottomLeft.X, BottomLeft.Y);
                dragPoints.Points[topLeftIdx] = new ScatterPoint(TopLeft.X, TopLeft.Y);
                dragPoints.Points[topRightIdx] = new ScatterPoint(TopRight.X, TopRight.Y);
                dragPoints.Points[bottomRightIdx] = new ScatterPoint(BottomRight.X, BottomRight.Y);
                samplingBox.MinimumX = BottomLeft.X;
                samplingBox.MinimumY = BottomLeft.Y;
                samplingBox.MaximumX = TopRight.X;
                samplingBox.MaximumY = TopRight.Y;
                Model.InvalidatePlot(false);
            });
        }

        // Destructor
        ~ScanDisplayControlViewModel()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        // Private methods
        void SendUpdate()
        {
            BottomLeft = new PositionCoordinate(dragPoints.Points[bottomLeftIdx].X, dragPoints.Points[bottomLeftIdx].Y);
            TopLeft = new PositionCoordinate(dragPoints.Points[topLeftIdx].X, dragPoints.Points[topLeftIdx].Y);
            TopRight = new PositionCoordinate(dragPoints.Points[topRightIdx].X, dragPoints.Points[topRightIdx].Y);
            BottomRight = new PositionCoordinate(dragPoints.Points[bottomRightIdx].X, dragPoints.Points[bottomRightIdx].Y);
            WeakReferenceMessenger.Default.Send<ScanAreaSelectionMessage>(new ScanAreaSelectionMessage((BottomLeft, TopLeft, TopRight, BottomRight)));
        }

        private ViewResolvingPlotModel CustomPlot()
        {
            var model = new ViewResolvingPlotModel();
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

            double centerX = _stageConfig.XAxisLength / 2;
            double centerY = _stageConfig.YAxisLength / 2;
            int defaultBoxSize = 50;
            samplingBox = new RectangleAnnotation
            {

                MinimumX = centerX - defaultBoxSize,
                MaximumX = centerX + defaultBoxSize,
                MinimumY = centerY - defaultBoxSize,
                MaximumY = centerY + defaultBoxSize,
                Fill = OxyColors.Transparent,
                Stroke = OxyColors.Blue,
                StrokeThickness = 2,
                Layer = AnnotationLayer.BelowSeries
            };
            model.Annotations.Add(samplingBox);

            dragPoints = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Green,
                MarkerSize = 6,
            };
            ScatterPoint bottomLeft = new(samplingBox.MinimumX, samplingBox.MinimumY);
            ScatterPoint topLeft = new(samplingBox.MinimumX, samplingBox.MaximumY);
            ScatterPoint topRight = new(samplingBox.MaximumX, samplingBox.MaximumY);
            ScatterPoint bottomRight = new(samplingBox.MaximumX, samplingBox.MinimumY);
            dragPoints.Points.Add(bottomLeft);
            dragPoints.Points.Add(topLeft);
            dragPoints.Points.Add(topRight);
            dragPoints.Points.Add(bottomRight);
            

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
                    SendUpdate();
                    Model.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

            dragPoints.MouseUp += (s, e) =>
            {
                indexOfPointToMove = -1;
                var point = dragPoints.InverseTransform(e.Position);
                SendUpdate();
                model.InvalidatePlot(false);
                e.Handled = true;
            };

            model.Series.Add(dragPoints);

            return model;
        }
    }
}
