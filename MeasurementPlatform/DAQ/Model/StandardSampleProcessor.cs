using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Interfaces;
using Microsoft.Extensions.Logging;

namespace DAQ.Model;
public class StandardSampleProcessor : ISampleProcessor
{
    private const double _mmPerCount = 0.00125;
    private readonly ILogger<StandardSampleProcessor> _logger;

    public StandardSampleProcessor(ILogger<StandardSampleProcessor> logger)
    {
        _logger = logger;
    }


    private (int XOriginCount, int YOriginCount) _scanEncoderOrigin;
    public (int XOriginCount, int YOriginCount) ScanEncoderOrigin
    {
        get => _scanEncoderOrigin;
        set
        {
            _scanEncoderOrigin = value;
            _logger.LogInformation("SampleProcessor encoder origin set to ({X},{Y})", value.XOriginCount, value.YOriginCount);
        } 
    }

    public ProcessedSample ProcessSample(RawSample sample)
    {
        double xcoordinate = (sample.XCounts - ScanEncoderOrigin.XOriginCount) * _mmPerCount;
        double ycoordinate = (sample.YCounts - ScanEncoderOrigin.YOriginCount) * _mmPerCount;
        double zcoordinate = VoltsToZCoordinates(sample.Voltage);
        return new ProcessedSample(xcoordinate, ycoordinate, zcoordinate);
    }

    private double VoltsToZCoordinates(double volts)
    {
        return (1.0f / (0.023f * volts + 0.0046f)) - 8;
    }
}
