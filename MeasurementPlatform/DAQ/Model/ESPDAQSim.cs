using DAQ.Enums;
using DAQ.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Model
{
    public class SimDAQ
    {
        private readonly ILogger<SimDAQ> _logger;

        public event EventHandler<ResponseReceivedEventArgs>? ResponseReceived;
        private int _delayMs;

        public SimDAQ(ILogger<SimDAQ> bottomLogger)
        {
            _logger = bottomLogger;
            _logger.LogInformation("Constructed SimDAQ");
            _delayMs = 100;
        }


        public void ReceiveCommand(Command cmd)
        {
            _logger.LogInformation($"Received Command: {cmd.ToString()}");
            switch (cmd.MessageType) 
            {
                case MessageType.Initialize:
                    HandleInitialize();
                    break;
                case MessageType.Deinitialize:
                    HandleDeinitialize();
                    break;
                case MessageType.GetVoltage:
                    HandleGetVoltage();
                    break;
                case MessageType.GetEncoderCounts:
                    HandleGetEncoderCounts();
                    break;
                case MessageType.ResetEncoder:
                    HandleResetEncoder();
                    break;
                default:
                    throw new Exception("Invalid MessageType");
            }


        }

        private void HandleResetEncoder()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delayMs);
                RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new Response(MessageType.ResetEncoder, ErrorCode.Success)));
            });
            
        }

        private void HandleGetEncoderCounts()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delayMs);
                RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new DataResponse<int>(MessageType.GetEncoderCounts, ErrorCode.Success, 1234)));
            });
        }

        private void HandleGetVoltage()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delayMs);
                RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new DataResponse<float>(MessageType.GetVoltage, ErrorCode.Success, 3.141592f)));
            });            
        }

        private void HandleDeinitialize()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delayMs);
                RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new Response(MessageType.Deinitialize, ErrorCode.Success)));
            });            
        }

        private void HandleInitialize()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delayMs);
                RaiseResponseReceivedEvent(new ResponseReceivedEventArgs(new Response(MessageType.Initialize, ErrorCode.Success)));
            });
        }

        protected virtual void RaiseResponseReceivedEvent(ResponseReceivedEventArgs e)
        {
            if(ResponseReceived != null)
            {
                EventHandler<ResponseReceivedEventArgs> handler = ResponseReceived;
                handler(this, e);
            }
        }

    }


    public class ESPDAQControllerSim
    {
        private readonly ILogger<ESPDAQControllerSim> _logger;
        private readonly SimDAQ _simDAQ;
        private bool _initCommandPending;
        private Command? _currentCommand;

        public event EventHandler<DAQStateEventArgs>? StateChanged;
        public event EventHandler<ResponseReceivedEventArgs>? CommandComplete;

        private bool _isInitialized;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                _isInitialized = value;
                OnStateChanged(new DAQStateEventArgs(_isInitialized ? DAQState.Initialized : DAQState.Uninitialized));
            }
        }


        public ESPDAQControllerSim(ILogger<ESPDAQControllerSim> middleLogger, ILogger<SimDAQ> bottomLogger)
        {
            _logger = middleLogger;
            _simDAQ = new SimDAQ(bottomLogger);
            _simDAQ.ResponseReceived += _simDAQ_ResponseReceived;
            _initCommandPending = false;
            IsInitialized = false;
        }

        private void _simDAQ_ResponseReceived(object? sender, ResponseReceivedEventArgs e)
        {
            if (e.Response != null)
            {
                if (_currentCommand != null && _currentCommand.MessageType == e.Response.MessageType)
                {
                    if (_initCommandPending)
                        CheckInitialized(e);
                    _initCommandPending = false;

                    OnCommandComplete(e);
                }
                else // either theres no current command or the response type doesnt match the command type, either way this is unexpected feedback
                {
                    // TODO handle unexpected feedback
                }
            }
        }

        public void SendCommand(Command cmd)
        {
            if (cmd.MessageType == MessageType.Initialize || cmd.MessageType == MessageType.Deinitialize) // mark init command pending if the request was either init or deinit
                _initCommandPending = true;
            if (cmd.MessageType == MessageType.Initialize)
                OnStateChanged(new DAQStateEventArgs(DAQState.Initializing));
            _simDAQ.ReceiveCommand(cmd);
            _currentCommand = cmd;
        }

        private void CheckInitialized(ResponseReceivedEventArgs e)
        {
            if (e.Response.MessageType == MessageType.Initialize) // responding to an init command
            {
                if (e.Response.ErrorCode == ErrorCode.Success || e.Response.ErrorCode == ErrorCode.AlreadyInitialized)
                    IsInitialized = true; // in these cases we are initialized
                else
                    IsInitialized = false; // any other error codes for init mean DAQ is not initialized
            }
            if (e.Response.MessageType == MessageType.Deinitialize)
            {
                IsInitialized = false; // regardless of success it means DAQ is deinitialized
            }
        }



        protected virtual void OnStateChanged(DAQStateEventArgs e)
        {
            if(StateChanged != null)
            {
                EventHandler<DAQStateEventArgs> handler = StateChanged;
                handler(this, e);

            }
        }

        protected virtual void OnCommandComplete(ResponseReceivedEventArgs e)
        {
            if(CommandComplete != null)
            {
                EventHandler<ResponseReceivedEventArgs> handler = CommandComplete;
                handler(this, e);
            }
        }

    }



    public class ESPDAQSim : IDAQ
    {
        private readonly ESPDAQControllerSim _controller;
        private readonly ILogger<ESPDAQSim> _logger;

        public bool Initialized => _controller.IsInitialized;

        public event EventHandler<DAQStateEventArgs>? StateChanged
        {
            add => this._controller.StateChanged += value;
            remove => this._controller.StateChanged -= value;
        }


        public ESPDAQSim(ILogger<ESPDAQSim> topLogger, ILogger<ESPDAQControllerSim> middleLogger, ILogger<SimDAQ> bottomLogger)
        {
            _logger = topLogger;
            _controller = new ESPDAQControllerSim(middleLogger, bottomLogger);
        }

        public async Task Deinitialize()
        {
            _logger.LogInformation("Deinitializing DAQ");
            await SendCommand(new Command(MessageType.Deinitialize));
        }

        public async Task<Tuple<int, int>> GetEncoderCounts()
        {
            _logger.LogInformation("Getting encoder counts");
            return await SendDataCommand<Tuple<int, int>>(new Command(MessageType.GetEncoderCounts));
        }

        public async Task<float> GetVolts()
        {
            _logger.LogInformation("Getting voltage");
            return await SendDataCommand<float>(new Command(MessageType.GetVoltage));
        }

        public async Task Initialize()
        {
            _logger.LogInformation("Initializing DAQ");
            await SendCommand(new Command(MessageType.Initialize));
        }

        public async Task ResetEncoder()
        {
            _logger.LogInformation("Resetting encoder counts");
            await SendCommand(new Command(MessageType.ResetEncoder));
        }

        // private methods
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
}
