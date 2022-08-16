using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Model;
namespace DAQ.Interfaces
{
    public interface IDAQ
    {
        bool Initialized { get; } // returns whether DAQ is currently initialized or not

        bool IsStreaming { get; }
        
        event EventHandler<DAQStateEventArgs>? StateChanged;

        Task Initialize(); // async method to initialize DAQ
        Task Deinitialize();
        Task<float> GetVolts();
        Task<Tuple<int, int>> GetEncoderCounts();
        Task ResetEncoder();

        Task StartStream(int sampleRate);
        Task StopStream();
    }
}
