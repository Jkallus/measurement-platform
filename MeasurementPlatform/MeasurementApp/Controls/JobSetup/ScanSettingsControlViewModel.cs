using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.JobSetup
{
    public class ScanSettingsControlViewModel: ObservableRecipient
    {
        // Private member variables
        private readonly ILogger<ScanSettingsControlViewModel> _logger;
        private readonly IServiceProvider _service;


        // Public properties
        private PositionCoordinate _bottomLeft;
        public PositionCoordinate BottomLeft
        {
            get => _bottomLeft;
            set => SetProperty(ref _bottomLeft, value);
        }

        private PositionCoordinate _topLeft;
        public PositionCoordinate TopLeft
        {
            get => _topLeft;
            set => SetProperty(ref _topLeft, value);
        }

        private PositionCoordinate _topRight;
        public PositionCoordinate TopRight
        {
            get => _topRight;
            set => SetProperty(ref _topRight, value);
        }

        private PositionCoordinate _bottomRight;
        public PositionCoordinate BottomRight
        {
            get => _bottomRight;
            set => SetProperty(ref _bottomRight, value);
        }



        // Constructor
        public ScanSettingsControlViewModel(IServiceProvider service, ILogger<ScanSettingsControlViewModel> logger)
        {
            _service = service;
            _logger = logger;
            WeakReferenceMessenger.Default.Register<ScanAreaSelectionMessage>(this, (r, m) =>
            {
                App.MainRoot.DispatcherQueue.TryEnqueue(() =>
                {
                    BottomLeft = m.Value.bottomLeft;
                    TopLeft = m.Value.topLeft;
                    TopRight = m.Value.topRight;
                    BottomRight = m.Value.bottomRight;
                });
                
            });
        }
    }
}
