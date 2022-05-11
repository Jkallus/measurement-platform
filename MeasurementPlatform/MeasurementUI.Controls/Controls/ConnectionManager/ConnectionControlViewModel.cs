using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAQ.Model;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class ConnectionControlViewModel : ObservableObject
    {
        #region Private Members
        private readonly SystemController _systemController;
        private readonly IMessageBoxService _messageBox;
        #endregion

        #region Constructor
        public ConnectionControlViewModel(SystemController systemController, IMessageBoxService messageBox)
        {
            _systemController = systemController;
            _messageBox = messageBox;
            _systemController.PropertyChanged += SystemController_PropertyChanged;
            ConnectCommand = new AsyncRelayCommand(OnConnect, CanConnect);
            DisconnectCommand = new AsyncRelayCommand(OnDisconnect, CanDisconnect);
        }
        #endregion

        #region Public Properties
        public IAsyncRelayCommand ConnectCommand { get; set; }
        public IAsyncRelayCommand DisconnectCommand { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if(SetProperty(ref isBusy, value))
                {
                    ConnectCommand.NotifyCanExecuteChanged();
                    DisconnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string MotionControllerStatus
        {
            get { return _systemController.MotionControllerStatus; }
        }

        public string DAQStatus
        {
            get { return _systemController.DAQStatus; }
        }


        #endregion

        #region Private Methods

        private void SystemController_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName); // assumes property names are same in model and viewmodel and viewmodel is using model as backing
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
                _messageBox.ShowMessageBox(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanConnect()
        {
            if (_systemController.IsMotionControllerConnected)
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
        #endregion
    }
}
