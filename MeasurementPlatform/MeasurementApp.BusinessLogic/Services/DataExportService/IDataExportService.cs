using MeasurementApp.BusinessLogic.SystemControl;

namespace MeasurementApp.BusinessLogic.Services.DataExportService;

public interface IDataExportService
{
    public void Export(Job job);
}
