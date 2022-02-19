using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeasurementUI.BusinessLogic.Configuration;
using MeasurementUI.BusinessLogic.SystemControl;
using MeasurementUI.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;

        public RelayCommand ConnectCommand { get; }

        public StagePositioningControlViewModel StagePositioningControlViewModel { get; private set; }


        public MainWindowViewModel(IDialogService dialogService,
                                   IConfiguration configuration,
                                   MachineConfiguration machineConf,
                                   SystemController systemController,
                                   IServiceProvider serivceProvider)
        {
            _dialogService = dialogService;
            _configuration = configuration;
            _machineConfiguration = machineConf;
            _systemController = systemController;
            _serviceProvider = serivceProvider;
            
            ConnectCommand = new RelayCommand(Connect);

            StagePositioningControlViewModel = _serviceProvider.GetRequiredService<StagePositioningControlViewModel>();
            
        }


        private void Connect()
        {
            _dialogService.ShowDialog<ConnectionControlViewModel>(result =>
            {
                var test = result;
            });
        }
    }
}
