using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json;
using StageControl;
using StageControl.Model;
using MeasurementUI.Core.Models;

namespace MeasurementUI.BusinessLogic.Configuration
{
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
