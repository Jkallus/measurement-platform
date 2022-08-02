using MeasurementApp.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MeasurementApp.BusinessLogic.SystemControl;

public class JobRunner: ObservableObject
{
    // Private members
    private readonly IServiceProvider _service;
    private readonly ILogger<JobRunner> _logger;
    private readonly SystemController? _controller;

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

    public JobRunner(IServiceProvider service, ILogger<JobRunner> logger)
    {
        _service = service;
        _logger = logger;
        _controller = _service.GetService(typeof(SystemController)) as SystemController ?? throw new ArgumentNullException("service");
    }

    public async Task ExecuteJob(CancellationToken token, IProgress<double> progress)
    {
        token.ThrowIfCancellationRequested();
        if (Job != null)
        {
            IsRunning = true;
            _logger.LogInformation("Job Starting: {Recipe}", Job.Recipe.Name);
            List<(double x, double y)> locs = Job.Recipe.GetScanPoints();
            Job.Result.ResultData.Clear();
            int i = 0;
            foreach (var loc in locs)
            {
                if(token.IsCancellationRequested)
                {
                    IsRunning = false;
                    token.ThrowIfCancellationRequested();
                }
                
                await _controller!.MoveTo(loc.x, loc.y, StageControl.Model.BlockingType.ExternallyBlocking, this); // TODO add better controller null check
                var counts = await _controller.GetEncoderCounts(this);
                double? volts = await _controller.GetVolts(this);
                progress.Report(100.0 * (double)i / (double)locs.Count);
                if (counts != null && volts != null)
                {
                    Job.Result.ResultData.Add(new Sample(counts.Item1, counts.Item2, volts.Value));
                }
                i++;
            }
        }
        else
        {
            // TODO add job null case
        }
        IsRunning = false;
    }
}
