using System.Threading.Tasks.Dataflow;
using DAQ.Model;
using MeasurementApp.BusinessLogic.Recipe;
using MeasurementApp.BusinessLogic.Services.DataExportService;
using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StageControl.Model;

namespace MeasurementApp.BusinessLogic.SystemControl;

public class JobRunner: ObservableObject
{
    // Private members
    private readonly IServiceProvider _service;
    private readonly ILogger<JobRunner> _logger;
    private readonly SystemController? _controller;
    private readonly IDataExportService _dataExport;

    // Public properties
    private Job? _job;
    public Job? Job
    {
        get => _job;
        set => SetProperty(ref _job, value);
    }
    
    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public JobRunner(IServiceProvider service, ILogger<JobRunner> logger, IDataExportService dataExport, SystemController systemController)
    {
        _service = service;
        _logger = logger;
        _dataExport = dataExport;
        _controller = systemController;
    }

    public async Task ExecuteJob(CancellationToken token, IProgress<double> progress)
    {
        token.ThrowIfCancellationRequested();
        if (Job != null)
        {
            IsRunning = true;
            int i = 0;
            _logger.LogInformation("Job Starting: {Recipe}", Job.Recipe.Name);
            Job.Result.Data.Clear();

            ActionBlock<ProcessedSample> recordBlock = new ActionBlock<ProcessedSample>((ProcessedSample sample) =>
            {
                Job.Result.Data.Add(sample);
            });
            IDisposable link = _controller!.DAQ.Stream.LinkTo(recordBlock);

            // Go to starting position
            List<PositionCoordinate> positions = Job.Recipe.GetPositions();
            PositionCoordinate finalPosition = positions[positions.Count - 1];
            positions.RemoveAt(positions.Count - 1); // save and remove last position 

            await _controller!.MoveTo(Job.Recipe.ScanStartLocation.AsTuple(), StageControl.Model.BlockingType.ExternallyBlocking, this);
            await _controller!.DAQ.StartStream(Job.Recipe.SamplingRate);
            foreach (var position in positions) // feed n - 1 positions to FluidNC for parsing. This should finish quickly depending on number of positions
            {
                await _controller!.MoveTo(position.AsTuple(), BlockingType.NonBlocking, this);
                progress.Report(100.0 * (double)i++ / (double)(positions.Count + 1));
            }

            await _controller!.MoveTo(finalPosition.AsTuple(), BlockingType.ExternallyBlocking, this); // wait on move to final position to know when scan is done

            await _controller!.DAQ.StopStream();
            link.Dispose();
            _dataExport.Export(Job);
        }
        else
        {
            // TODO add job null case
        }
        IsRunning = false;
    }
}
