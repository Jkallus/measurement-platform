using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DAQ.Enums;
using DAQ.Interfaces;
using MeasurementApp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DAQ.Model;

public class ESPDAQ: IDAQ
{
    // Private members
    private readonly ESPDAQController _controller;
    private readonly ILogger _logger;


    // Public properties
    public event EventHandler<DAQStateEventArgs>? StateChanged
    {
        add => this._controller.StateChanged += value;
        remove => this._controller.StateChanged -= value;
    }

    public bool Initialized => _controller.IsInitialized;
    public bool IsStreaming => _controller.IsStreaming;

    // Constructor
    public ESPDAQ(DAQSerialConfig serialConfig, ILogger<ESPDAQ> topLogger, ILogger<ESPDAQController> middleLogger, ILogger<SerialController> bottomLogger)
    {
        _logger = topLogger;
        _controller = new ESPDAQController(serialConfig, middleLogger, bottomLogger);
    }

    // Public methods
    public async Task Initialize()
    {
        _logger.LogInformation("Initializing DAQ");
        await SendCommand(new Command(MessageType.Initialize));
    }

    public async Task Deinitialize()
    {
        _logger.LogInformation("Deinitializing DAQ");
        await SendCommand(new Command(MessageType.Deinitialize));
    }

    public async Task<float> GetVolts()
    {
        _logger.LogInformation("Getting voltage");
        return await SendDataCommand<float>(new Command(MessageType.GetVoltage));
    }

    public async Task<Tuple<int,int>> GetEncoderCounts()
    {
        _logger.LogInformation("Getting encoder counts");
        return await SendDataCommand<Tuple<int,int>>(new Command(MessageType.GetEncoderCounts));
    }

    public async Task ResetEncoder()
    {
        _logger.LogInformation("Resetting encoder counts");
        await SendCommand(new Command(MessageType.ResetEncoder));
    }

    public async Task StartStream(int sampleRate)
    {
        _logger.LogInformation("Starting streaming at {SampleRate} Hz", sampleRate);
        await SendCommand<int>(new ParameterCommand<int>(MessageType.StartStream, sampleRate));
    }
    public async Task StopStream()
    {
        _logger.LogInformation("Stopping streaming");
        await SendCommand(new Command(MessageType.StopStream));
    }

    // private methods
    private async Task SendCommand<T>(ParameterCommand<T> cmd)
    {
        TaskCompletionSource tcs = new TaskCompletionSource();
        EventHandler<ResponseReceivedEventArgs>? CommandCompleteEventHandler = null;
        CommandCompleteEventHandler = (sender, e) =>
        {
            _controller.CommandComplete -= CommandCompleteEventHandler;
            if (e.Response.ErrorCode == ErrorCode.Success)
            {
                tcs.SetResult();
            }
            else
            {
                tcs.SetException(new DAQException(e.Response.ErrorCode));
            }
        };
        _controller.CommandComplete += CommandCompleteEventHandler;
        _controller.SendCommand(cmd);
        await tcs.Task;
    }


    private async Task SendCommand(Command cmd)
    {
        TaskCompletionSource tcs = new TaskCompletionSource();
        EventHandler<ResponseReceivedEventArgs>? CommandCompleteEventHandler = null;
        CommandCompleteEventHandler = (sender, e) =>
        {
            _controller.CommandComplete -= CommandCompleteEventHandler;
            if (e.Response.ErrorCode == ErrorCode.Success)
            {
                tcs.SetResult();
            }
            else if(e.Response.ErrorCode == ErrorCode.AlreadyInitialized)
            {
                _logger.LogWarning("DAQ already initialized. Previous disconnect did not deinitialize DAQ");
                tcs.SetResult();
            }
            else
            {
                tcs.SetException(new DAQException(e.Response.ErrorCode));
            }
        };
        _controller.CommandComplete += CommandCompleteEventHandler;
        _controller.SendCommand(cmd);
        await tcs.Task;
    }

    private async Task<T> SendDataCommand<T>(Command cmd)
    {
        TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
        EventHandler<ResponseReceivedEventArgs>? CommandCompleteEventHandler = null;
        CommandCompleteEventHandler = (sender, e) =>
        {
            _controller.CommandComplete -= CommandCompleteEventHandler;
            if (e.Response.ErrorCode == ErrorCode.Success)
            {
                DataResponse<T>? result = e.Response as DataResponse<T>;
                tcs.SetResult(result!.Data);
            }
            else
            {
                tcs.SetException(new DAQException(e.Response.ErrorCode));
            }
        };
        _controller.CommandComplete += CommandCompleteEventHandler;
        _controller.SendCommand(cmd);
        return await tcs.Task;
    }
}
