using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Core.Contracts.Services;

public interface IMessageBoxService
{
    void ShowMessageBox(string message);
    bool ShowQuestionMessageBox(string message);
}
