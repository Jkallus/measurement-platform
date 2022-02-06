using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeasurementUI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
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
