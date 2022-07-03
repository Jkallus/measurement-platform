using MeasurementUI.BusinessLogic.Recipe;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class JobRunner: ObservableObject
    {
        // Private members
        private readonly IServiceProvider _service;
        private readonly ILogger<JobRunner> _logger;
        private readonly SystemController? _controller;
        private bool _stopQueued;
        private event EventHandler<EventArgs> jobStopped;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

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
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;
            _controller = _service.GetService(typeof(SystemController)) as SystemController ?? throw new ArgumentNullException("service");
        }

        public async Task ExecuteJob(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Job != null)
            {
                IsRunning = true;
                _logger.LogInformation("Job Starting: {Recipe}", Job.Recipe.Name);
                List<(double x, double y)> locs = Job.Recipe.GetScanPoints();
                Job.Result.ResultData.Clear();
                foreach (var loc in locs)
                {
                    if(token.IsCancellationRequested)
                    {
                        IsRunning = false;
                        token.ThrowIfCancellationRequested();
                    }
                   
                    await _controller!.MoveTo(loc.x, loc.y, StageControl.Model.BlockingType.ExternallyBlocking, this); // TODO add better controller null check
                    var counts = await _controller.GetEncoderCounts(this);
                    var volts = await _controller.GetVolts(this);
                    if (counts != null && volts != null)
                    {
                        Job.Result.ResultData.Add(new Sample(counts.Item1, counts.Item2, volts.Value));
                    }
                }
            }
            else
            {
                // TODO add job null case
            }
            IsRunning = false;
        }

        protected void OnJobStopped(EventArgs e)
        {
            if(jobStopped != null)
            {
                jobStopped(this, e);
            }
        }
    }
}
