using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeasurementUI.BusinessLogic.SystemControl;
using System;
using System.Collections.Generic;
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
            ConnectCommand = new RelayCommand(OnConnect, CanConnect);
            DisconnectCommand = new RelayCommand(OnDisconnect, CanDisconnect);
        }
        #endregion

        #region Public Properties
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }


        public string MotionControllerStatus
        {
            get { return ""; }
            set { }
        }
        #endregion

        #region Private Methods
        private void OnConnect()
        {

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
