using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeasurementUI.BusinessLogic.SystemControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class ConnectionControlViewModel: ObservableObject
    {
        #region Private Members
        private readonly SystemController _systemController;
        #endregion

        #region Constructor
        public ConnectionControlViewModel(SystemController systemController)
        {
            _systemController = systemController;
            _systemController.PropertyChanged += SystemController_PropertyChanged;
            ConnectCommand = new AsyncRelayCommand(OnConnect, CanConnect);
            DisconnectCommand = new RelayCommand(OnDisconnect, CanDisconnect);
        }
        #endregion

        #region Public Properties
        public IAsyncRelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }


        public string MotionControllerStatus
        {
            get { return _systemController.MotionControllerStatus; }
        }
        #endregion

        #region Private Methods

        private void SystemController_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName); // assumes property names are same in model and viewmodel and viewmodel is using model as backing
        }


        private async Task OnConnect()
        {
            await _systemController.Initialize();
        }

        private bool CanConnect()
        {
            return true;
        }

        private void OnDisconnect()
        {

        }

        private bool CanDisconnect()
        {
            return true;
        }
        #endregion
    }
}
