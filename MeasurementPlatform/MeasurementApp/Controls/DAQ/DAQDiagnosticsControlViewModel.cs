using DAQ.Interfaces;
using DAQ.Model;
using MeasurementApp.Services;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace MeasurementApp.Controls;

public class DAQDiagnosticsControlViewModel: ObservableObject
{
    // Private member variables
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DAQDiagnosticsControlViewModel> _logger;
    private readonly IDAQ _daq;
    private readonly ActionBlock<ProcessedSample> _displayBlock;
    private IDisposable? _link;
    

    // Public Properties
    public IAsyncRelayCommand InitializeCommand { get; set; }
    public IAsyncRelayCommand DeinitializeCommand { get; set; }
    public IAsyncRelayCommand GetVoltageCommand { get; set; }
    public IAsyncRelayCommand GetCountCommand { get; set; }
    public IAsyncRelayCommand ResetEncoderCommand { get; set; }
    public IAsyncRelayCommand GetScaledValueCommand { get; set; }
    public IAsyncRelayCommand StartStreamCommand { get; set; }
    public IAsyncRelayCommand StopStreamCommand { get; set; }

    private int _sampleRate;
    public int SampleRate
    {
        get => _sampleRate;
        set => SetProperty(ref _sampleRate, value);
    }

    private double _scaledValue;
    public double ScaledValue
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

    private double _xcoordinate;
    public string XCoordinate => $"{_xcoordinate.ToString("0.000")} mm";
    
    private double _ycoordinate;
    public string YCoordinate =>  $"{_ycoordinate.ToString("0.000")} mm";

    // Constructor
    public DAQDiagnosticsControlViewModel(IServiceProvider serviceProvider, ILogger<DAQDiagnosticsControlViewModel> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _daq = ((_serviceProvider.GetService(typeof(SystemController)) as SystemController) ?? throw new Exception("System controller is null")).DAQ; // grab DAQ instance from the systemcontroller
        _displayBlock = new ActionBlock<ProcessedSample>((ProcessedSample sample) =>
        {
            App.MainRoot!.DispatcherQueue.TryEnqueue(() =>
            {
                _xcoordinate = sample.XCoordinate;
                _ycoordinate = sample.YCoordinate;
                ScaledValue = sample.Z;
                OnPropertyChanged("XCoordinate");
                OnPropertyChanged("YCoordinate");
            });
        });

        _voltage = double.NaN;
        _sampleRate = 0;
        _scaledValue = 0;
        _xcoordinate = 0;
        _ycoordinate = 0;
        _xcount = 0;
        _ycount = 0;

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

        _logger.LogInformation("Finished constructing DAQDiagnosticsControlViewModel");
    }



    // Private methods

    private async Task StartStream(int sampleRate)
    {
        try
        {
            _link = _daq.Stream.LinkTo(_displayBlock);
            await _daq.StartStream(sampleRate);
            //await Task.Run(async () =>
            //{
            //    while(_daq.IsStreaming)
            //    {
            //        ProcessedSample sample = await _daq.Stream.ReceiveAsync();
            //        App.MainRoot!.DispatcherQueue.TryEnqueue(async () =>
            //        {
                        
            //            XCoordinate = sample.XCoordinate;
            //            YCoordinate = sample.YCoordinate;
            //            ScaledValue = sample.Z;
            //        });
            //    }
            //});
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
            _link!.Dispose();
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
        return _daq.Initialized && !_daq.IsStreaming;
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
        return 10 - ((1.0f / (0.023f * volts + 0.0046f)) - 8);
    }

    private bool CanResetEncoder()
    {
        return _daq.Initialized && !_daq.IsStreaming;
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
        return _daq.Initialized && !_daq.IsStreaming;
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
        return _daq.Initialized && !_daq.IsStreaming;
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
        return _daq.Initialized && !_daq.IsStreaming;
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
        return !_daq.Initialized && !_daq.IsStreaming;
    }
}
