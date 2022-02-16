using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeasurementUI.BusinessLogic.Configuration;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
    {
        private readonly IDialogService _dialogService;
        private readonly IConfiguration _configuration;
        private readonly MachineConfiguration _machineConfiguration;
        private readonly ISystemController _systemController;

        public RelayCommand ConnectCommand { get; }


        public MainWindowViewModel(IDialogService dialogService, IConfiguration configuration, MachineConfiguration machineConf, ISystemController systemController)
        {
            _dialogService = dialogService;
            _configuration = configuration;
            _machineConfiguration = machineConf;
            _systemController = systemController;
            
            ConnectCommand = new RelayCommand(Connect);

            test();
        }


        private void Connect()
        {
            _dialogService.ShowDialog<ConnectionControlStubViewModel>(result =>
            {
                var test = result;
            });
        }

        private void test()
        {
            System.Diagnostics.Debug.WriteLine(_machineConfiguration.SerialConfig.COM);
        }
    }
}
