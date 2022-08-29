using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Interfaces;
using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DAQ.Model;
public class StandardSampleProcessor : ISampleProcessor
{
    private const double _mmPerCount = 0.00125;
    private readonly ILogger<StandardSampleProcessor> _logger;
    private readonly StageConfig _stageConfig;
    private (double XHomeCoordinate, double YHomeCoordinate) _encoderHomePosition;

    public StandardSampleProcessor(ILogger<StandardSampleProcessor> logger, StageConfig stageConfig)
    {
        _logger = logger;
        _stageConfig = stageConfig;
        _encoderHomePosition = new(_stageConfig.XHomePosition, _stageConfig.YHomePosition);
    }

    public ProcessedSample ProcessSample(RawSample sample)
    {
        double xcoordinate = _encoderHomePosition.XHomeCoordinate + (sample.XCounts * _mmPerCount); // 300.000 + (-8000 * 0.00125)
        double ycoordinate = _encoderHomePosition.YHomeCoordinate + (sample.YCounts * _mmPerCount); // 0 + (8000 * 0.00125)
        double zcoordinate = VoltsToZCoordinates(sample.Voltage);
        return new ProcessedSample(xcoordinate, ycoordinate, zcoordinate);
    }

    private double VoltsToZCoordinates(double volts)
    {
        return 10 - ((1.0f / (0.023f * volts + 0.0046f)) - 8);
    }
}
