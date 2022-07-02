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

        // Public properties
        private Job? _job;
        public Job? Job
        {
            get => _job;
            set => SetProperty(ref _job, value);
        }


        public JobRunner(IServiceProvider service, ILogger<JobRunner> logger)
        {
            _service = service;
            _logger = logger;
            _controller = _service.GetService(typeof(SystemController)) as SystemController ?? throw new ArgumentNullException("service");
        }

        public async Task ExecuteJob()
        {
            _logger.LogInformation("Job Starting: {Recipe}", Job.Recipe.Name);
            List<(double x, double y)> locs = Job.Recipe.GetScanPoints();
            foreach(var loc in locs)
            {
                //_controller.MotionController.G
            }
        }
    }
}
