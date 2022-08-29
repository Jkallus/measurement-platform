using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MeasurementApp.BusinessLogic.Services.FilepathFinderService;
public class DefaultFilepathFinder : IPathfinder
{
    private readonly IConfiguration _config;
    private readonly string MEASUREAPP_DIR;
    public DefaultFilepathFinder(IConfiguration config)
    {
        _config = config;
        MEASUREAPP_DIR = _config.GetSection("MEASUREAPP_DIR").Value;
    }

    public string DataPath => $"{MEASUREAPP_DIR}\\data";

    public string RecipePath => $"{MEASUREAPP_DIR}\\recipes";
}
