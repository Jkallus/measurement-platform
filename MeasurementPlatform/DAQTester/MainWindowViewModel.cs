using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DAQ.Model;
using DAQ.Interfaces;
using MeasurementUI.Core.Interfaces;
using MeasurementUI.Core.Services;

namespace DAQTester
{
    public class MainWindowViewModel: ObservableObject
    {
        private IDAQ _daq;
        private IMessageBoxService _messageBox;
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

        private long _count;
        public long Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }


        private double _voltage;
        public double Voltage
        {
            get { return _voltage; }
            set { SetProperty(ref _voltage, value); }
        }

        public MainWindowViewModel()
        {
            _daq = new ESPDAQ();
            _messageBox = new MessageBoxService();
            InitializeCommand = new AsyncRelayCommand(Initialize, CanInitialize);
            DeinitializeCommand = new AsyncRelayCommand(Deinitialize, CanDeinitialize);
            GetVoltageCommand = new AsyncRelayCommand(GetVoltage, CanGetVoltage);
            GetCountCommand = new AsyncRelayCommand(GetCount, CanGetCount);
            ResetEncoderCommand = new AsyncRelayCommand(ResetEncoder, CanResetEncoder);
            GetScaledValueCommand = new AsyncRelayCommand(GetScaledValue, CanGetScaledValue);

            Initialized = _daq.Initialized;
            Voltage = 0.0;
        }

        private bool CanGetScaledValue()
        {
            return _daq.Initialized;
        }

        public async Task GetScaledValue()
        {
            try
            {
                float x = await _daq.GetVolts();
                ScaledValue = ApplyScale(x);
            }
            catch (DAQException ex)
            {
                _messageBox.ShowMessageBox(ex.Message);
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
                _messageBox.ShowMessageBox(ex.Message);
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
                Count = await _daq.GetEncoderCounts();
            }
            catch (DAQException ex)
            {
                _messageBox.ShowMessageBox(ex.Message);
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
                _messageBox.ShowMessageBox(ex.Message);
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
                if(ex.DAQError == DAQ.Enums.ErrorCode.AlreadyInitialized)
                {
                    Initialized = true;
                }
                else
                {
                    _messageBox.ShowMessageBox(ex.Message);
                }
            }                        
        }

        private bool CanInitialize()
        {
            return !_daq.Initialized;
        }
    }
}
