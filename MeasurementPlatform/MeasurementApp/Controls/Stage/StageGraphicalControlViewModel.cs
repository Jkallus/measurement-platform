using MeasurementUI.BusinessLogic.SystemControl;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using StageControl.Events;
using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls
{
    public class StageGraphicalControlViewModel: ObservableObject
    {
        // Private member variables
        private readonly ILogger<StageGraphicalControlViewModel> _logger;
        private readonly IServiceProvider _service;
        private readonly SystemController _systemController;
        private readonly StageConfig _stageConfig;

        // Public properties
        private double _targetXCoordinate;
        public double TargetXCoordinate
        {
            get => _targetXCoordinate;
            set => SetProperty(ref _targetXCoordinate, value);
        }

        private double _targetYCoordinate;
        public double TargetYCoordinate
        {
            get => _targetYCoordinate;
            set => SetProperty(ref _targetYCoordinate, value);
        }

        public PlotModel Model { get; set; }

        public StageGraphicalControlViewModel(IServiceProvider serviceProvider, ILogger<StageGraphicalControlViewModel> logger)
        {
            _logger = logger;
            _service = serviceProvider;
            _systemController = _service.GetService(typeof(SystemController)) as SystemController;
            _stageConfig = _service.GetService(typeof(StageConfig)) as StageConfig;

            _systemController.MotionController.PositionChanged += MotionController_PositionChanged;

            Model = CustomPlot();
        }

        private void MotionController_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            App.MainRoot.DispatcherQueue.TryEnqueue(() =>
            {
                (Model.Series[0] as ScatterSeries).Points[0] = new ScatterPoint(e.X, e.Y);
                Model.InvalidatePlot(false);
            });
        }

        private PlotModel CustomPlot()
        {
            var model = new PlotModel();
            model.PlotType = PlotType.Cartesian;


            var l = new Legend
            {
                LegendSymbolLength = 40
            };
            model.Legends.Add(l);

            double edgeVisibilityOffset = 20;

            var xAxis = new LinearAxis();
            xAxis.Minimum = 0 - edgeVisibilityOffset;
            xAxis.Maximum = _stageConfig.XAxisLength + edgeVisibilityOffset;
            xAxis.AbsoluteMinimum = 0 - edgeVisibilityOffset;
            xAxis.AbsoluteMaximum = _stageConfig.XAxisLength + edgeVisibilityOffset;
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorStep = 50;
            xAxis.IsPanEnabled = false;
            xAxis.IsZoomEnabled = false;

            var yAxis = new LinearAxis();
            yAxis.Minimum = 0 - edgeVisibilityOffset;
            yAxis.Maximum = _stageConfig.YAxisLength + edgeVisibilityOffset;
            yAxis.AbsoluteMinimum = 0 - edgeVisibilityOffset;
            yAxis.AbsoluteMaximum = _stageConfig.YAxisLength + edgeVisibilityOffset;
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorStep = 50;
            yAxis.IsPanEnabled = false;
            yAxis.IsZoomEnabled = false;

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            var currentLocationPoints = new ScatterSeries();
            currentLocationPoints.Title = "Current Location";
            currentLocationPoints.Points.Add(new ScatterPoint(50, 50));
            currentLocationPoints.MarkerType = MarkerType.Circle;
            currentLocationPoints.MarkerFill = OxyColors.Blue;
            currentLocationPoints.MarkerSize = 6;
            currentLocationPoints.MarkerStrokeThickness = 1.5;
            model.Series.Add(currentLocationPoints);

            var targetLocationPoints = new ScatterSeries();
            targetLocationPoints.Title = "Target Location";
            targetLocationPoints.Points.Add(new ScatterPoint(10, 10));
            targetLocationPoints.MarkerType = MarkerType.Circle;
            targetLocationPoints.MarkerFill = OxyColors.Red;
            targetLocationPoints.MarkerSize = 6;
            targetLocationPoints.MarkerStrokeThickness = 1.5;
            model.Series.Add(targetLocationPoints);

            //ScatterPoint rectStartPoint;
            //ScatterPoint rectEndPoint;
            //RectangleAnnotation rectAnnotation;
            //bool currentlySelecting = false;

            int indexOfPointToMove = -1;

#pragma warning disable CS0618 // Type or member is obsolete
            targetLocationPoints.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
                    var nearestPoint = targetLocationPoints.Transform(targetLocationPoints.Points[indexOfNearestPoint].X, targetLocationPoints.Points[indexOfNearestPoint].Y);

                    if ((nearestPoint - e.Position).Length < 10)
                    {
                        indexOfPointToMove = indexOfNearestPoint;
                    }

                    targetLocationPoints.MarkerFill = OxyColors.Purple;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

            targetLocationPoints.MouseMove += (s, e) =>
            {
                if (indexOfPointToMove >= 0)
                {
                    var point = targetLocationPoints.InverseTransform(e.Position);
                    targetLocationPoints.Points[indexOfPointToMove] = new ScatterPoint(point.X, point.Y);
                    model.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

            targetLocationPoints.MouseUp += (s, e) =>
            {
                indexOfPointToMove = -1;
                var point = targetLocationPoints.InverseTransform(e.Position);
                targetLocationPoints.MarkerFill = OxyColors.Red;
                WeakReferenceMessenger.Default.Send<StageTargetPositionChangedMessage>(new StageTargetPositionChangedMessage(((float)point.X, (float)point.Y)));
                model.InvalidatePlot(false);
                e.Handled = true;
            };
#pragma warning restore CS0618 // Type or member is obsolete

            return model;
        }
    }
}
