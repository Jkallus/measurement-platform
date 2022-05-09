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

namespace DAQTester
{
    public class MainWindowViewModel: ObservableObject
    {
        private IDAQ daq;

        public IAsyncRelayCommand InitializeCommand { get; set; }
        public RelayCommand DeinitializeCommand { get; set; }
        public IAsyncRelayCommand GetVoltageCommand { get; set; }
        public IAsyncRelayCommand GetCountCommand { get; set; }
        public RelayCommand ResetEncoderCommand { get; set; }

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
            daq = new ESPDAQ();
            InitializeCommand = new AsyncRelayCommand(Initialize, CanInitialize);
            DeinitializeCommand = new RelayCommand(Deinitialize, CanDeinitialize);
            GetVoltageCommand = new AsyncRelayCommand(GetVoltage, CanGetVoltage);
            GetCountCommand = new AsyncRelayCommand(GetCount, CanGetCount);
            ResetEncoderCommand = new RelayCommand(ResetEncoder, CanResetEncoder);

            Initialized = daq.Initialized;
            Voltage = 0.0;
        }

        private bool CanResetEncoder()
        {
            return daq.Initialized;
        }

        private void ResetEncoder()
        {
            daq.ResetEncoder();
            //GetCount();
        }

        private void NotifyInitialized()
        {
            InitializeCommand.NotifyCanExecuteChanged();
            DeinitializeCommand.NotifyCanExecuteChanged();
            GetVoltageCommand.NotifyCanExecuteChanged();
            GetCountCommand.NotifyCanExecuteChanged();
            ResetEncoderCommand.NotifyCanExecuteChanged();
        }

        private bool CanGetCount()
        {
            return true; // daq.Initialized;
        }

        private async Task GetCount()
        {
            try
            {
                Count = await daq.GetEncoderCounts();
            }
            catch (DAQException ex)
            {

            }
        }

        private bool CanDeinitialize()
        {
            return daq.Initialized;
        }

        private void Deinitialize()
        {
            daq.Deinitialize();
            Initialized = false;
        }

        private async Task GetVoltage()
        {
            try
            {
                Voltage = await daq.GetVolts();
            }
            catch (DAQException ex)
            {

            }
            
        }

        private bool CanGetVoltage()
        {
            //return daq.Initialized;
            return true;
        }

        private async Task Initialize()
        {
            try
            {
                await daq.Initialize();
                Initialized = true;
            }
            catch (DAQException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            
        }

        private bool CanInitialize()
        {
            return !daq.Initialized;
        }
    }
}
