using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls;

public class StageTargetPositionChangedMessage: ValueChangedMessage<(float XCoordinate, float YCoordinate)>
{
    public StageTargetPositionChangedMessage((float XCoordinate, float YCoordinate) value) : base(value)
    {
        TargetLocation = value;
    }

    public (float XCoordinate, float YCoordinate) TargetLocation { get; set; }
    
}
