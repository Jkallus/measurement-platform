using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json;

namespace MeasurementUI.BusinessLogic
{
    public class SerialConfig
    {
        public string COM { get; set; }
        public int BaudRate { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public int DataBits { get; set; }
        public System.IO.Ports.StopBits StopBits { get; set; }

        public SerialConfig()
        {
            this.COM = String.Empty;
        }
    }

    public class StageConfig
    {
        public double XAxisLength { get; set; }
        public double YAxisLength { get; set; }
    }


    public class MachineConfiguration
    {
        public SerialConfig SerialConfig { get; set; }
        public StageConfig StageConfig { get; set; }

        public MachineConfiguration()
        {
            this.SerialConfig = new SerialConfig();
            this.StageConfig = new StageConfig();
        }


        public static MachineConfiguration LoadConfiguration(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            MachineConfiguration conf = JsonSerializer.Deserialize<MachineConfiguration>(json);
            return conf;
        }


        public void SaveToJSON(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string outputJson = JsonSerializer.Serialize<MachineConfiguration>(this, options);
            File.WriteAllText(path, outputJson);
        }

    }
}
