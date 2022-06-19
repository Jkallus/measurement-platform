using DAQ.Model;
using MeasurementApp.Services;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.BusinessLogic.SystemControl.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls
{
    public class ConnectionControlViewModel : ObservableObject
    {
        // Private Members
        private readonly SystemController _systemController;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConnectionControlViewModel> _logger;

        // Constructor
        public ConnectionControlViewModel(IServiceProvider serviceProvider, ILogger<ConnectionControlViewModel> logger)
        {
            _serviceProvider = serviceProvider;
            _systemController = _serviceProvider.GetService(typeof(SystemController)) as SystemController;
            _systemController.PropertyChanged += SystemController_PropertyChanged;
            ConnectCommand = new AsyncRelayCommand(OnConnect, CanConnect);
            DisconnectCommand = new AsyncRelayCommand(OnDisconnect, CanDisconnect);
            _logger = logger;
        }

        // Public Properties
        public IAsyncRelayCommand ConnectCommand { get; set; }
        public IAsyncRelayCommand DisconnectCommand { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if(SetProperty(ref _isBusy, value))
                {
                    ConnectCommand.NotifyCanExecuteChanged();
                    DisconnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ModuleInitializationState DAQInitializationState { get => _systemController.DAQInitializationState; }
        public ModuleInitializationState MotionControllerInitializationState { get => _systemController.MotionControllerInitializationState; }

        public string MotionControllerStatus
        {
            get => _systemController.MotionControllerStatus;
        }

        public string DAQStatus
        {
            get => _systemController.DAQStatus;
        }


        // Private Methods
        private void SystemController_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            App.MainRoot.DispatcherQueue.TryEnqueue(() =>
            {
                OnPropertyChanged(e.PropertyName); // assumes property names are same in model and viewmodel and viewmodel is using model as backing
            });
        }

        private async Task OnConnect()
        {
            try
            {
                IsBusy = true;
                _logger.LogInformation("Connecting to system");
                await _systemController.Initialize();
            }
            catch(DAQException ex)
            {
                _logger.LogError(ex, "Error on connect");
                await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
            }
            catch(FileNotFoundException ex)
            {
                _logger.LogDebug(ex.Message);
                await App.MainRoot.MessageDialogAsync("Serial Port Error", ex.Message);
             }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanConnect()
        {
            if(_systemController.IsMotionControllerConnected)
                return false;
            else if (IsBusy)
                return false;
            else
                return true;
        }

        private async Task OnDisconnect()
        {
            await _systemController.Deinitialize();
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }

        private bool CanDisconnect()
        {
            if (!_systemController.IsMotionControllerConnected)
                return false;
            else if (IsBusy)
                return false;
            else
                return true;
        }
    }
}
