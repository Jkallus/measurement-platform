using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementApp.Controls;

public class ReadoutItem: ObservableObject
{
    private string _field;
    public string Field
    {
        get => _field;
        set => SetProperty(ref _field, value);
    }

    private string _value;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public ReadoutItem(string field, string value)
    {
        _field = field;
        _value = value;
    }
}

public class PositionReadoutControlViewModel: ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemController _systemController;

    private ObservableCollection<ReadoutItem> _items;
    public ObservableCollection<ReadoutItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public PositionReadoutControlViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _systemController = _serviceProvider.GetService(typeof(SystemController)) as SystemController ?? throw new Exception("SystemController is null");
        _items = new ObservableCollection<ReadoutItem>();
        Items.Add(new ReadoutItem("X Position", "Unknown"));
        Items.Add(new ReadoutItem("Y Position", "Unknown"));
        Items.Add(new ReadoutItem("Stage Homed", _systemController.MotionController.IsHomed.ToString()));
        
        _systemController.MotionController.PositionChanged += MotionController_PositionChanged;
        _systemController.MotionController.HomingComplete += MotionController_HomingComplete;
        _systemController.MotionController.StateChanged += MotionController_StateChanged;
    }

    private void MotionController_StateChanged(object? sender, StageControl.Events.FNCStateChangedEventArgs e)
    {
        App.MainRoot!.DispatcherQueue.TryEnqueue(() =>
        {
            Items[0].Value = "Unknown";
            Items[1].Value = "Unknown";
            Items[2].Value = _systemController.MotionController.IsHomed.ToString();
        });
    }

    private void MotionController_HomingComplete(object? sender, EventArgs e)
    {
        App.MainRoot!.DispatcherQueue.TryEnqueue(() =>
        {
            Items[2].Value = _systemController.MotionController.IsHomed.ToString();
        });
        
    }

    private void MotionController_PositionChanged(object? sender, StageControl.Events.PositionChangedEventArgs e)
    {
        App.MainRoot!.DispatcherQueue.TryEnqueue(() =>
        {
            Items[0].Value = e.X.ToString("0.000");
            Items[1].Value = e.Y.ToString("0.000");
        });
        
    }
}
