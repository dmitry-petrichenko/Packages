using System;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.Commands;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface IReceiveSession : IDisposable
    {
        Task<bool> Validate();
        void SendCommand(byte[] bytes);
        
        event Action<byte[]> Received;
        event Action Closed;
    }
    
    public class ReceiveSession : IReceiveSession
    {
        private readonly INetworkTunnel _networkTunnel;
        private readonly IRecorder _recorder;

        private TaskCompletionSource<bool> _validateResult;
        private SessionState _state;
        
        public ReceiveSession(INetworkTunnel networkTunnel, IRecorder recorder)
        {
            _recorder = recorder;
            _validateResult = new TaskCompletionSource<bool>();
            
            _networkTunnel = networkTunnel;
            _networkTunnel.Closed += TunnelClosedHandler;
            _networkTunnel.Received += BytesReceivedHandler;
            _networkTunnel.Listen();
            
            _state = SessionState.LOGIN;
        }

        public void Dispose()
        {
            _networkTunnel.Closed -= TunnelClosedHandler;
            _networkTunnel.Received -= BytesReceivedHandler;
            _networkTunnel.Dispose();
        }

        public Task<bool> Validate()
        {
            return _validateResult.Task;
        }

        public void SendCommand(byte[] bytes)
        {
            SafeExecution.TryCatch(() => SendCommandInternal(bytes), ExceptionHandler);
        }
        
        public void SendCommandInternal(byte[] bytes)
        {
            if (_state != SessionState.SEANCE)
            {
                _recorder.RecordError(nameof(ReceiveSession), $"trying to send command is state {_state}");
                return;
            }

            if (_networkTunnel == null) 
            {
                _recorder.RecordError(nameof(ReceiveSession), $"trying to send command when networkTunnel is NULL");
                return;
            }
            
            SendInternal(bytes);
        }

        public event Action<byte[]> Received;
        public event Action Closed;
        
        
        private void BytesReceivedHandler(byte[] bytes)
        {
            SafeExecution.TryCatch(() => BytesReceivedHandlerInternal(bytes), ExceptionHandler);
        }

        private void BytesReceivedHandlerInternal(byte[] bytes)
        {
            _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler {bytes.Length}");
            switch (_state)
            {
                case SessionState.LOGIN:
                {
                    _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler SessionState.LOGIN");
                    if (bytes.Length != 6)
                    {
                        _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler RESPONSE_LOGIN_FAIL");
                        SendInternal(NodeCommands.RESPONSE_LOGIN_FAIL.ToBytesArray());
                        _validateResult.SetResult(false);
                        return;
                    }
                    
                    var login = Encoding.ASCII.GetString(bytes);
                    if (!login.Equals("aninel"))
                    {
                        _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler RESPONSE_LOGIN_FAIL");
                        SendInternal(NodeCommands.RESPONSE_LOGIN_FAIL.ToBytesArray());
                        _validateResult.SetResult(false);
                        return;
                    }

                    _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler LOGIN_SUCCESS");
                    _state = SessionState.SEANCE;
                    _validateResult.SetResult(true);
                    SendInternal(NodeCommands.RESPONSE_LOGIN_SUCCESS.ToBytesArray());
                    
                    return;
                }
                case SessionState.SEANCE:
                {
                    var result = CommandFilter.TryGetInnerData(bytes, (byte)NodeCommands.COMMAND_PREFIX);
                    _recorder.RecordInfo(nameof(ReceiveSession), $"BytesReceivedHandler SessionState.SEANCE belong to command {result.Item1}:");

                    if (!result.Item1)
                    {
                        SendInternal(NodeCommands.RESPONSE_COMMAND_WRONG.ToBytesArray());
                        return;
                    }

                    SafeExecution.TryCatch(()=>Received?.Invoke(result.Item2), EventExceptionHandler);
                    return;
                }
                default:
                {
                    Closed?.Invoke();
                    return;
                }
            }
        }

        private void SendInternal(byte[] command)
        {
            var result = CommandFilter.WrapDataWithPrefix(command, (byte) NodeCommands.RESPONSE_PREFIX);
            _networkTunnel.Send(result);
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder?.RecordError(nameof(ReceiveSession), exception.Message);
        }
        
        private void EventExceptionHandler(Exception exception)
        {
            _recorder?.RecordError($"{nameof(ReceiveSession)} |Event Thread Exception|", exception.Message);
        }

        private void TunnelClosedHandler()
        {
            Closed?.Invoke();
        }

        private enum SessionState
        {
            LOGIN,
            SEANCE
        }
    }
}