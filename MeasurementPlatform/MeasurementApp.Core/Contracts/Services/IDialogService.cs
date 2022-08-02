using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Core.Contracts.Services;
public interface IDialogService
{
    void ShowDialog(string name, Action<string> callback);
    void ShowDialog<ViewModel>(Action<string> callback);
}