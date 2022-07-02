using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.BusinessLogic.SystemControl
{
    public class ResourceBusyException: Exception
    {
        public object CurrentOwner { get; set; }

        public ResourceBusyException(object currentOwner): base($"{currentOwner.ToString()} is currently using this resource")
        {
            CurrentOwner = currentOwner;
        }
    }
}
