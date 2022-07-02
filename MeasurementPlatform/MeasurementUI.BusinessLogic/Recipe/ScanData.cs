using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Recipe
{
    public struct Sample
    {
        int Xcounts { get; set; }
        int Ycounts { get; set; }
        double Volts { get; set; }

        double ScaledValue
        {
            get => (1.0f / (0.023f * Volts + 0.0046f)) - 8;
        }

        public Sample(int xcounts, int ycounts, double volts)
        {
            Xcounts = xcounts;
            Ycounts = ycounts;
            Volts = volts;
        }
    }

    public class ScanData
    {
        private List<Sample> _resultData;
        public List<Sample> ResultData
        {
            get => _resultData;
            set => _resultData = value;
        }

        public ScanData(int initialCapacity = 0)
        {
            _resultData = new List<Sample>(initialCapacity);
        }
    }
}
