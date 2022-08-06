using DAQ.Interfaces;
using MeasurementApp.BusinessLogic.Configuration;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StageControl.Enums;
using StageControl.Events;
using StageControl.Interfaces;
using StageControl.Model;
using DAQ.Model;
using DAQ.Enums;
using MeasurementApp.BusinessLogic.SystemControl.Enums;
using Microsoft.Extensions.Logging;

namespace MeasurementApp.BusinessLogic.SystemControl;

public class SystemController: ObservableObject, ISystemController
{
    // Private members
    private readonly IServiceProvider _serviceProvider;
    private readonly MachineConfiguration _machineConfiguration;
    private readonly ILogger<SystemController> _systemLogger;
    private bool _hwBusy;
    private object? _hwOwner;
   
    
    // Public properties
    public readonly IMachineControl MotionController;
    public readonly IDAQ DAQ;

    private string _motionControllerStatus;
    public string MotionControllerStatus
    {
        get => _motionControllerStatus;
        private set => SetProperty(ref _motionControllerStatus, value);
    }

    private ModuleInitializationState _daqInitializationState;
    public ModuleInitializationState DAQInitializationState
    {
        get => _daqInitializationState;
        set => SetProperty(ref _daqInitializationState, value);
    }

    private ModuleInitializationState _motionControllerInitializationState;
    public ModuleInitializationState MotionControllerInitializationState
    {
        get => _motionControllerInitializationState;
        set => SetProperty(ref _motionControllerInitializationState, value);
    }

    private string _daqStatus;
    public string DAQStatus
    {
        get => _daqStatus;
        private set => SetProperty(ref _daqStatus, value);
    }

    public bool IsMotionControllerConnected => MotionController.IsConnected;


    // Constructor
    public SystemController(IServiceProvider serviceProvider, ILogger<SystemController> logger)
    {
        _serviceProvider = serviceProvider;
        _systemLogger = logger;
        _systemLogger.LogInformation("System Controller object constructed");

        _machineConfiguration = _serviceProvider.GetService(typeof(MachineConfiguration)) as MachineConfiguration ?? throw new ArgumentNullException("MachineConfiguration", "MachineConfiguration instance was null");
        
        MotionController = _serviceProvider.GetService(typeof(IMachineControl)) as IMachineControl ?? throw new ArgumentNullException("MachineController", "MachineController instance was null"); ;
        MotionController.StateChanged += MotionController_StateChanged;
        MotionController.UnexpectedRestart += MotionController_UnexpectedRestart;
        _motionControllerStatus = FNCMachineControl.ConvertControllerStateToStatus(LifetimeFNCState.Unknown);

        DAQ = _serviceProvider.GetService(typeof(IDAQ)) as IDAQ ?? throw new ArgumentNullException("IDAQ", "IDAQ instance was null");
        DAQ.StateChanged += DAQ_StateChanged;
        _daqStatus = (new DAQStateEventArgs(DAQState.Uninitialized)).ToFriendlyString();

        _hwBusy = false;
        _hwOwner = null;
        _systemLogger.LogInformation("Constructor finished");
    }

    public async Task Home(HomingAxes axes, object caller)
    {
        if(_hwBusy)
        {
            throw new ResourceBusyException(_hwOwner!);
        }
        else
        {
            _hwBusy = true;
            _hwOwner = caller;
            _systemLogger.LogInformation("Homing {Axis}", axes.ToString());
            await MotionController.Home(axes);
            _hwBusy = false;
            _hwOwner = null;
        }
    }

    public async Task Jog(int x, int y, JogType type, object caller)
    {
        if (_hwBusy)
        {
            throw new ResourceBusyException(_hwOwner!);
        }
        else
        {
            _hwBusy = true;
            _hwOwner = caller;
            _systemLogger.LogInformation("Jogging {Xcoords},{Ycoords},{JogType}", x, y, type.ToString());
            await MotionController.Jog(x, y, type);
            _hwBusy = false;
            _hwOwner = null;
        }
    }

    public async Task MoveTo(double x, double y, BlockingType blocking, object caller)
    {
        if(_hwBusy)
        {
            _systemLogger.LogWarning("Resource requested by {Caller} but in use by {Owner}", caller.ToString(), _hwOwner!.ToString());
            throw new ResourceBusyException(_hwOwner!);
        }
        else
        {
            _hwBusy = true;
            _hwOwner = caller;
            _systemLogger.LogInformation("Moving to {Xcoords}, {Ycoords} in {Blocking} move",x, y, blocking.ToString());
            if(blocking == BlockingType.ExternallyBlocking)
            {
                await MotionController.MoveTo(x, y);
            }
            else
            {
                await MotionController.MoveToNonBlocking(x, y);
            }
            _hwBusy = false;
            _hwOwner = null;
        }
    }

    public async Task Initialize(object caller)
    {
        try
        {
            _hwBusy = true;
            _hwOwner = caller;
            _systemLogger.LogInformation("Initializing system");
            var task1 = MotionController.Initialize();
            var task2 = DAQ.Initialize();
            await Task.WhenAll(task1, task2);
        }
        catch (DAQException ex)
        {
            if (ex.DAQError != ErrorCode.AlreadyInitialized)
            {
                _systemLogger.LogError(ex, "Caught DAQError");
                throw ex;
            }
        }
        finally
        {
            _hwBusy = false;
            _hwOwner = null;
        }
    }

    public async Task Deinitialize(object caller)
    {
        _hwBusy = true;
        _hwOwner = caller;
        _systemLogger.LogInformation("Deinitializing system");
        MotionController.Deinitialize();
        await DAQ.Deinitialize();
        _hwBusy = false;
        _hwOwner = caller;
    }

    public async Task<Tuple<int, int>?> GetEncoderCounts(object caller)
    {
        Tuple<int, int>? counts;
        try
        {
            _hwBusy = true;
            _hwOwner = caller;
            counts = await DAQ.GetEncoderCounts();
        }
        catch(DAQException ex)
        {
            _systemLogger.LogError(ex, "Caught DAQError");
            throw ex;
        }
        finally
        {
            _hwBusy = false;
            _hwOwner = null;
        }

        return counts;
    }

    public async Task<double?> GetVolts(object caller)
    {
        double? volts;
        try
        {
            _hwBusy = true;
            _hwOwner = caller;
            volts = await DAQ.GetVolts();
        }
        catch(DAQException ex)
        {
            _systemLogger.LogError(ex, "Caught DAQError");
            throw ex;
        }
        finally
        {
            _hwBusy = false;
            _hwOwner = null;
        }

        return volts;
    }

    // Event handlers
    private void MotionController_UnexpectedRestart(object? sender, EventArgs e)
    {
        
    }

    private void MotionController_StateChanged(object? sender, FNCStateChangedEventArgs e)
    {
        MotionControllerStatus = FNCMachineControl.ConvertControllerStateToStatus(e.State);
        if(e.State == LifetimeFNCState.Unknown)
        {
            MotionControllerInitializationState = ModuleInitializationState.Uninitialized;
        }
        else if(e.State == LifetimeFNCState.FNCReady)
        {
            MotionControllerInitializationState = ModuleInitializationState.Initialized;
        }
        else
        {
            MotionControllerInitializationState = ModuleInitializationState.Initializing;
        }
    }

    private void DAQ_StateChanged(object? sender, DAQStateEventArgs e)
    {
        DAQStatus = e.ToFriendlyString();
        switch(e.State)
        {
            case DAQState.Uninitialized:
                DAQInitializationState = ModuleInitializationState.Uninitialized;
                break;
            case DAQState.Initializing:
                DAQInitializationState = ModuleInitializationState.Initializing;
                break;
            case DAQState.Initialized:
                DAQInitializationState = ModuleInitializationState.Initialized;
                break;
        }
    }

    private void MotionController_PositionChanged(object? sender, PositionChangedEventArgs e)
    {
        
    }
}