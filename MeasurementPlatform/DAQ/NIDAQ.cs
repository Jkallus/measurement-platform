using NationalInstruments.DAQmx;
using NITask = NationalInstruments.DAQmx.Task;

namespace DAQ
{
    public class NIDAQ
    {
        private NITask? voltTask;
        private NITask? countTask;
        private AnalogSingleChannelReader? analogReader;
        private CounterSingleChannelReader? counterReader;

        private bool initialized;
        public bool Initialized
        {
            get { return initialized; }
            set { initialized = value; }
        }

        public NIDAQ()
        {
            initialized = false;
        }

        public void Initialize()
        {
            SetupVolts();
            SetupCount();
            Initialized = true;
        }

        private void SetupCount()
        {
            try
            {
                countTask = new NITask("CountTask");
                countTask.CIChannels.CreateAngularEncoderChannel("Dev1/ctr0", "LinearCount", CIEncoderDecodingType.X4, false, 0, CIEncoderZIndexPhase.AHighBHigh, 600 , 0, CIAngularEncoderUnits.Degrees);
                countTask.Timing.ConfigureSampleClock("/Dev1/PFI10", 1000, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);
                counterReader = new CounterSingleChannelReader(countTask.Stream);
                countTask.Control(TaskAction.Verify);
                countTask.Start();
            }
            catch (DaqException ex)
            {
                throw ex;
            }
        }

        private void SetupVolts()
        {
            try
            {
                voltTask = new NITask("VoltTask");
                voltTask.AIChannels.CreateVoltageChannel("Dev1/ai0", "", AITerminalConfiguration.Rse, 0, 3.5, AIVoltageUnits.Volts);
                analogReader = new AnalogSingleChannelReader(voltTask.Stream);
                voltTask.Control(TaskAction.Verify);
                voltTask.Start();
            }
            catch (DaqException ex)
            {
                throw ex;
            }
        }

        public void ResetCounter()
        {
            if(countTask != null)
                countTask.Dispose();
            countTask = null;
            SetupCount();
        }

        public long GetCounterValue()
        {
            if (countTask != null)
                return countTask.CIChannels["LinearCount"].Count;
            else
                throw new Exception("NIDAQ not initialized properly");
        }

        public double GetScaledValue()
        {
            if (counterReader != null)
            {
                return counterReader.ReadSingleSampleDouble(); // degree units
            }
            else
                throw new Exception("NIDAQ not initialized properly");
        }

        public double GetVolts()
        {
            if (analogReader != null)
                return analogReader.ReadSingleSample();
            else
                throw new Exception("NIDAQ not initialized");
        }

        public void Deinitialize()
        {
            if (voltTask != null)
            {
                voltTask.Dispose();
            }
            if(countTask != null)
                countTask.Dispose();
            Initialized = false;
        }
    }
}