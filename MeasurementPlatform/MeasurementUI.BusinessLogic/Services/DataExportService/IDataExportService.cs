using MeasurementUI.BusinessLogic.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.Services.DataExportService
{
    public interface IDataExportService
    {
        public void Export(ScanData data);
    }
}
