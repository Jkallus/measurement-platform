using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DAQ;

namespace DAQTester
{
    public class MainWindowViewModel: ObservableObject
    {
        //private NIDAQ daq;
        private ESPDAQ daq;

        public RelayCommand InitializeCommand { get; set; }
        public RelayCommand DeinitializeCommand { get; set; }
        public RelayCommand GetVoltageCommand { get; set; }
        public RelayCommand GetCountCommand { get; set; }
        public RelayCommand ResetCounterCommand { get; set; }
        public RelayCommand GetScaledValueCommand { get; set; }


        private double _scaledCounterValue;
        public double ScaledCounterValue
        {
            get { return _scaledCounterValue; }
            set
            {
                SetProperty(ref _scaledCounterValue, value);
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
            daq = new ESPDAQ();
            InitializeCommand = new RelayCommand(Initialize, CanInitialize);
            DeinitializeCommand = new RelayCommand(Deinitialize, CanDeinitialize);
            GetVoltageCommand = new RelayCommand(GetVoltage, CanGetVoltage);
            GetCountCommand = new RelayCommand(GetCount, CanGetCount);
            ResetCounterCommand = new RelayCommand(ResetCounter, CanResetCounter);
            GetScaledValueCommand = new RelayCommand(GetScaledValue, CanGetScaledValue);

            Initialized = daq.Initialized;
            Voltage = 0.0;
        }

        private bool CanGetScaledValue()
        {
            return daq.Initialized;
        }

        private void GetScaledValue()
        {
            ScaledCounterValue = daq.GetScaledValue();
        }

        private bool CanResetCounter()
        {
            return daq.Initialized;
        }

        private void ResetCounter()
        {
            daq.ResetCounter();
            GetCount();
        }

        private void NotifyInitialized()
        {
            InitializeCommand.NotifyCanExecuteChanged();
            DeinitializeCommand.NotifyCanExecuteChanged();
            GetVoltageCommand.NotifyCanExecuteChanged();
            GetCountCommand.NotifyCanExecuteChanged();
            ResetCounterCommand.NotifyCanExecuteChanged();
            GetScaledValueCommand.NotifyCanExecuteChanged();
        }

        private bool CanGetCount()
        {
            return daq.Initialized;
        }

        private void GetCount()
        {
            Count = daq.GetCounterValue();
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

        private void GetVoltage()
        {
            Voltage = daq.GetVolts();
        }

        private bool CanGetVoltage()
        {
            return daq.Initialized;
        }

        private void Initialize()
        {
            daq.Initialize();
            Initialized = true;
        }

        private bool CanInitialize()
        {
            return !daq.Initialized;
        }
    }
}
