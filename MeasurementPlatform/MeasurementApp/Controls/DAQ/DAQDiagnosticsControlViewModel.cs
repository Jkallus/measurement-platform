using DAQ.Interfaces;
using DAQ.Model;
using MeasurementApp.Services;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls;

public class DAQDiagnosticsControlViewModel: ObservableObject
{
    // Private member variables
    private readonly IServiceProvider _serviceProvider;
    private IDAQ _daq;


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
    }

    // Public Properties
    public IAsyncRelayCommand InitializeCommand { get; set; }
    public IAsyncRelayCommand DeinitializeCommand { get; set; }
    public IAsyncRelayCommand GetVoltageCommand { get; set; }
    public IAsyncRelayCommand GetCountCommand { get; set; }
    public IAsyncRelayCommand ResetEncoderCommand { get; set; }
    public IAsyncRelayCommand GetScaledValueCommand { get; set; }

    private float _scaledValue;
    public float ScaledValue
    {
        get => _scaledValue;
        set
        {
            SetProperty(ref _scaledValue, value);
        }
    }


    private bool _initialized;
    public bool Initialized
    {
        get { return _initialized; }
        set
        {
            SetProperty(ref _initialized, value);
            NotifyInitialized();
        }
    }

    private long _xcount;
    public long XCount
    {
        get { return _xcount; }
        set { SetProperty(ref _xcount, value); }
    }

    private long _ycount;
    public long YCount
    {
        get { return _ycount; }
        set { SetProperty(ref _ycount, value); }
    }


    private double _voltage;
    public double Voltage
    {
        get { return _voltage; }
        set { SetProperty(ref _voltage, value); }
    }

    // Private methods
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
            await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
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
            await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private void NotifyInitialized()
    {
        InitializeCommand.NotifyCanExecuteChanged();
        DeinitializeCommand.NotifyCanExecuteChanged();
        GetVoltageCommand.NotifyCanExecuteChanged();
        GetCountCommand.NotifyCanExecuteChanged();
        ResetEncoderCommand.NotifyCanExecuteChanged();
        GetScaledValueCommand.NotifyCanExecuteChanged();
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
            await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
        }
    }

    private bool CanDeinitialize()
    {
        return _daq.Initialized;
    }

    private async Task Deinitialize()
    {
        await _daq.Deinitialize();
        Initialized = false;
    }

    private async Task GetVoltage()
    {
        try
        {
            Voltage = await _daq.GetVolts();
        }
        catch (DAQException ex)
        {
            await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
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
            Initialized = true;
        }
        catch (DAQException ex)
        {
            if (ex.DAQError == DAQ.Enums.ErrorCode.AlreadyInitialized)
            {
                Initialized = true;
            }
            else
            {
                await App.MainRoot.MessageDialogAsync("DAQError", ex.Message);
            }
        }
    }

    private bool CanInitialize()
    {
        return !_daq.Initialized;
    }
}
