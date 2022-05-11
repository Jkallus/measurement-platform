using MeasurementUI.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUI.Controls.ViewModels
{
    public class ReadoutItem: ObservableObject
    {
        private string _field;
        public string Field
        {
            get { return _field; }
            set { SetProperty(ref _field, value); }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public ReadoutItem(string field, string value)
        {
            _field = field;
            _value = value;
        }
    }


    public class PositionReadoutControlViewModel: ObservableRecipient
    {
        private readonly SystemController _systemController;


        private ObservableCollection<ReadoutItem> _items;
        public ObservableCollection<ReadoutItem> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public PositionReadoutControlViewModel(SystemController systemController)
        {
            _systemController = systemController;
            _items = new ObservableCollection<ReadoutItem>();
            double? initialXPosition = _systemController.IsMotionControllerConnected ? _systemController.MotionController.XPosition : null;
            double? initialYPosition = _systemController.IsMotionControllerConnected? _systemController.MotionController.YPosition : null;
            Items.Add(new ReadoutItem("X Position", initialXPosition.ToString()));
            Items.Add(new ReadoutItem("Y Position", initialYPosition.ToString()));

            _systemController.MotionController.PositionChanged += MotionController_PositionChanged;
        }

        private void MotionController_PositionChanged(object? sender, StageControl.Events.PositionChangedEventArgs e)
        {
            Items[0].Value = e.X.ToString("0.000");
            Items[1].Value = e.Y.ToString("0.000");
        }
    }
}
