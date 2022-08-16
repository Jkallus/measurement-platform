using DAQ.Interfaces;
using DAQ.Model;
using MeasurementApp.Services;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace MeasurementApp.Controls;

public class DAQDiagnosticsControlViewModel: ObservableObject
{
    // Private member variables
    private readonly IServiceProvider _serviceProvider;
    private readonly IDAQ _daq;


    // Public Properties
    public IAsyncRelayCommand InitializeCommand
    {
        get; set;
    }
    public IAsyncRelayCommand DeinitializeCommand
    {
        get; set;
    }
    public IAsyncRelayCommand GetVoltageCommand
    {
        get; set;
    }
    public IAsyncRelayCommand GetCountCommand
    {
        get; set;
    }
    public IAsyncRelayCommand ResetEncoderCommand
    {
        get; set;
    }
    public IAsyncRelayCommand GetScaledValueCommand
    {
        get; set;
    }
    public IAsyncRelayCommand StartStreamCommand
    {
        get; set;
    }
    public IAsyncRelayCommand StopStreamCommand
    {
        get; set;
    }

    private int _sampleRate;
    public int SampleRate
    {
        get => _sampleRate;
        set => SetProperty(ref _sampleRate, value);
    }

    private float _scaledValue;
    public float ScaledValue
    {
        get => _scaledValue;
        set => SetProperty(ref _scaledValue, value);
    }

    public bool Initialized => _daq.Initialized;

    private long _xcount;
    public long XCount
    {
        get => _xcount;
        set => SetProperty(ref _xcount, value);
    }

    private long _ycount;
    public long YCount
    {
        get => _ycount;
        set => SetProperty(ref _ycount, value);
    }

    private double _voltage;
    public double Voltage
    {
        get => _voltage;
        set => SetProperty(ref _voltage, value);
    }

    // Constructor
    public DAQDiagnosticsControlViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _daq = ((_serviceProvider.GetService(typeof(SystemController)) as SystemController) ?? throw new Exception("System controller is null")).DAQ; // grab DAQ instance from the systemcontroller
        InitializeCommand = new AsyncRelayCommand(Initialize, CanInitialize);
        DeinitializeCommand = new AsyncRelayCommand(Deinitialize, CanDeinitialize);
        GetVoltageCommand = new AsyncRelayCommand(GetVoltage, CanGetVoltage);
        GetCountCommand = new AsyncRelayCommand(GetCount, CanGetCount);
        ResetEncoderCommand = new AsyncRelayCommand(ResetEncoder, CanResetEncoder);
        GetScaledValueCommand = new AsyncRelayCommand(GetScaledValue, CanGetScaledValue);
        StartStreamCommand = new AsyncRelayCommand<int>(StartStream, CanStartStream);
        StopStreamCommand = new AsyncRelayCommand(StopStream, CanStopStream);

        _daq.StateChanged += (object? sender, DAQStateEventArgs e) =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                InitializeCommand.NotifyCanExecuteChanged();
                DeinitializeCommand.NotifyCanExecuteChanged();
                GetVoltageCommand.NotifyCanExecuteChanged();
                GetCountCommand.NotifyCanExecuteChanged();
                ResetEncoderCommand.NotifyCanExecuteChanged();
                GetScaledValueCommand.NotifyCanExecuteChanged();
                StartStreamCommand.NotifyCanExecuteChanged();
                StopStreamCommand.NotifyCanExecuteChanged();
                OnPropertyChanged("Initialized");
            });            
        };
    }



    // Private methods

    private async Task StartStream(int sampleRate)
    {
        try
        {
            await _daq.StartStream(sampleRate);
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanStartStream(int sampleRate)
    {
        return _daq.Initialized && !_daq.IsStreaming;
    }

    private async Task StopStream()
    {
        try
        {
            await _daq.StopStream();
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanStopStream()
    {
        return _daq.IsStreaming;
    }


    private bool CanGetScaledValue()
    {
        return _daq.Initialized;
    }

    private async Task GetScaledValue()
    {
        try
        {
            float x = await _daq.GetVolts();
            ScaledValue = ApplyScale(x);
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private float ApplyScale(float volts)
    {
        return (1.0f / (0.023f * volts + 0.0046f)) - 8;
    }

    private bool CanResetEncoder()
    {
        return _daq.Initialized;
    }

    private async Task ResetEncoder()
    {
        try
        {
            await _daq.ResetEncoder();
            await GetCount();
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanGetCount()
    {
        return _daq.Initialized;
    }

    private async Task GetCount()
    {
        try
        {
            var counts = await _daq.GetEncoderCounts();
            XCount = counts.Item1;
            YCount = counts.Item2;
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanDeinitialize()
    {
        return _daq.Initialized;
    }

    private async Task Deinitialize()
    {
        await _daq.Deinitialize();
    }

    private async Task GetVoltage()
    {
        try
        {
            Voltage = await _daq.GetVolts();
        }
        catch (DAQException ex)
        {
            await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanGetVoltage()
    {
        return _daq.Initialized;
    }

    private async Task Initialize()
    {
        try
        {
            await _daq.Initialize();
        }
        catch (DAQException ex)
        {
            if (ex.DAQError == DAQ.Enums.ErrorCode.AlreadyInitialized)
            {
                // TODO remove this case
            }
            else
            {
                await App.MainRoot!.MessageDialogAsync("DAQError", ex.Message);
            }
        }
    }

    private bool CanInitialize()
    {
        return !_daq.Initialized;
    }
}
