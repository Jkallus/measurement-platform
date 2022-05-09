using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Core.Interfaces
{
    public interface IMessageBoxService
    {
        void ShowMessageBox(string message);
        bool ShowQuestionMessageBox(string message);
    }
}
