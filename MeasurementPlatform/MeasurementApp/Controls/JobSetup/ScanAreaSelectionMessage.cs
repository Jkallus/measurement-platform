using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasurementApp.Core.Models;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace MeasurementApp.Controls.RecipeSetup;

public class ScanAreaSelectionMessage : ValueChangedMessage<(PositionCoordinate bottomLeft, PositionCoordinate topLeft, PositionCoordinate topRight, PositionCoordinate bottomRight)>
{
    
    PositionCoordinate BottomLeft { get; set; }
    PositionCoordinate TopLeft { get; set; }
    PositionCoordinate TopRight { get; set; }
    PositionCoordinate BottomRight { get; set; }
    public ScanAreaSelectionMessage((PositionCoordinate bottomLeft, PositionCoordinate topLeft, PositionCoordinate topRight, PositionCoordinate bottomRight) value) : base(value)
    {
        BottomLeft = value.bottomLeft;
        TopLeft = value.topLeft;
        TopRight = value.topRight;
        BottomRight = value.bottomRight;
    }
}
