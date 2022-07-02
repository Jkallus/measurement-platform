using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Recipe
{
    public class ScanData
    {
        private List<double> _resultData;
        public List<double> ResultData
        {
            get => _resultData;
            set => _resultData = value;
        }

        public ScanData(int initialCapacity = 0)
        {
            _resultData = new List<double>(initialCapacity);
        }
    }
}
