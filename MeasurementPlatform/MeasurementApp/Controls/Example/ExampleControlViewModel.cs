using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StageControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls
{
    public class ExampleControlViewModel: ObservableRecipient
    {

        // Private member variables
        private readonly IServiceProvider _service;
        private readonly StageConfig _stageConfig;
        private readonly ILogger<ExampleControlViewModel> _logger;

        // Public properties
        private string _myText;
        public string MyText
        {
            get {  return _myText; }
            set {  SetProperty(ref _myText, value); }
        }


        public ExampleControlViewModel(IServiceProvider service, ILogger<ExampleControlViewModel> logger)
        {
            _service = service;
            _stageConfig = _service.GetService(typeof(StageConfig)) as StageConfig;
            _logger = logger;
            MyText = _stageConfig.ToString();
            _logger.LogInformation("ExampleControlViewModel Constructed");
        }

        ~ExampleControlViewModel()
        {
            _logger.LogInformation("ExampleControlViewModel Destroyed");
        }

    }
}
