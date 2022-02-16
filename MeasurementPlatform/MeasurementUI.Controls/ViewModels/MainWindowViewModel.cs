using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public RelayCommand ConnectCommand { get; }


        public MainWindowViewModel(IDialogService dialogService, IConfiguration configuration)
        {
            _dialogService = dialogService;
            _configuration = configuration;
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
            System.Diagnostics.Debug.WriteLine(_configuration.GetSection("name").Value);
        }
    }
}
