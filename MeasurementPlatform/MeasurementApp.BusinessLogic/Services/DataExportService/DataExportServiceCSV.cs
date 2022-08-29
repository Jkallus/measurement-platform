using CsvHelper;
using MeasurementApp.BusinessLogic.Recipe;
using MeasurementApp.BusinessLogic.Services.FilepathFinderService;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.BusinessLogic.Services.DataExportService;

public class DataExportServiceCSV : IDataExportService
{
    private readonly ILogger<DataExportServiceCSV> _logger;
    private readonly IServiceProvider _service;
    private readonly IPathfinder _pathfinder;

    public DataExportServiceCSV(IServiceProvider service, ILogger<DataExportServiceCSV> logger, IPathfinder pathfinder)
    {
        _service = service;
        _logger = logger;
        _pathfinder = pathfinder;
    }

    public void Export(Job job)
    {
        string path = _pathfinder.DataPath;
        string filename = $"{job.Recipe.Name}_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.csv";
        using var writer = new StreamWriter(Path.Combine(path, filename));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        _logger.LogInformation($"Writing result of {job.Recipe.Name} to {_pathfinder.DataPath}");
        csv.WriteRecords(job.Result.Data);
    }
}
