using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Interfaces
{
    public interface IDAQ
    {
        bool Initialized { get; } // returns whether DAQ is currently initialized or not
        Task Initialize(); // async method to initialize DAQ
        Task Deinitialize();
        Task<float> GetVolts();
        Task<int> GetEncoderCounts();
        Task ResetEncoder();
    }
}
