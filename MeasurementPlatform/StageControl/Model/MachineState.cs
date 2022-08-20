using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementApp.Core.Models;

namespace StageControl.Model;

public class MachineState
{
    public AxisState XAxis;
    public AxisState YAxis;
    public StageConfig Config;

    public MachineState()
    {
        XAxis = new AxisState();
        YAxis = new AxisState();
        Config = new StageConfig();
    }

    public MachineState(StageConfig config)
    {
        XAxis = new AxisState();
        YAxis = new AxisState();
        Config = config;    
    }
}
