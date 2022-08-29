using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Services.FilepathFinderService;
public interface IPathfinder
{
    public string DataPath { get; }
    public string RecipePath { get; }
}
