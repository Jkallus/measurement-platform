using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json;
using StageControl;
using StageControl.Model;
using MeasurementApp.Core.Models;

namespace MeasurementApp.BusinessLogic.Configuration;

public class MachineConfiguration
{
    public SerialConfig StageSerialConfig { get; set; }
    public SerialConfig DAQSerialConfig { get; set; }
    public StageConfig StageConfig { get; set; }

    public MachineConfiguration()
    {
        this.StageSerialConfig = new SerialConfig();
        this.DAQSerialConfig = new SerialConfig();
        this.StageConfig = new StageConfig();
    }

    public void SaveToJSON(string path)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        string outputJson = JsonSerializer.Serialize<MachineConfiguration>(this, options);
        File.WriteAllText(path, outputJson);
    }

}
