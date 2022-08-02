using System;
using Microsoft.Toolkit.Mvvm;
using MeasurementApp.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using MeasurementApp.BusinessLogic.Configuration;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MeasurementApp.Services;

namespace MeasurementApp.ViewModels;

public class HomeViewModel : ObservableRecipient
{

    //private string _testText;
    //public string TestText
    //{
    //    get { return _testText; }
    //    set { SetProperty(ref _testText, value); }
    //}

    //public IAsyncRelayCommand DialogCommand;


    public HomeViewModel()
    {
        //TestText = App.GetService<MachineConfiguration>().DAQSerialConfig.COM;
        //DialogCommand = new AsyncRelayCommand(OnDialogButtonPressed);
    }

    //private async Task OnDialogButtonPressed()
    //{
    //    //await _messageBoxService.ShowMessageBox(root, "This is sample text");

    //    await App.MainRoot.MessageDialogAsync("Error", "Error message text here");
        
    //}
}
