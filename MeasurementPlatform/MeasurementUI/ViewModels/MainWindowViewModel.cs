using MeasurementUI.Controls.ViewModels;
using MeasurementUI.Core.Interfaces;
using MeasurementUI.Core.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.ViewModels
{
    public class MainWindowViewModel: ObservableRecipient
    {
        IDialogService dialogService;

        public RelayCommand ConnectCommand { get; }


        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            ConnectCommand = new RelayCommand(Connect);    
        }


        private void Connect()
        {
            dialogService.ShowDialog<ConnectionControlStubViewModel>(result =>
            {
                var test = result;
            });
        }
    }
}
