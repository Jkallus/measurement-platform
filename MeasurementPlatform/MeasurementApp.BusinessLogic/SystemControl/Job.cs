using MeasurementApp.BusinessLogic.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.SystemControl;

public class Job
{
    public ScanRecipe Recipe { get; set; }        
    public ScanData Result { get; set; }

    public Job(ScanRecipe recipe)
    {
        Recipe = recipe;
        Result = new ScanData();
    }
}
