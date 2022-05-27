using DAQ.Model;
using MeasurementApp.Services;
using MeasurementUI.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        // Constructor
        public ConnectionControlViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _systemController = _serviceProvider.GetService(typeof(SystemController)) as SystemController;
            _systemController.PropertyChanged += SystemController_PropertyChanged;
            ConnectCommand = new AsyncRelayCommand(OnConnect, CanConnect);
            DisconnectCommand = new AsyncRelayCommand(OnDisconnect, CanDisconnect);
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
                await _systemController.Initialize();
            }
            catch(DAQException ex)
            {
                await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
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
