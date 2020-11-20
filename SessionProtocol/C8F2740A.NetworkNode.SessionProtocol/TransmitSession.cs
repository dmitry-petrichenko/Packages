using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.Commands;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface ITransmitSession : IDisposable
    {
        Task<bool> Authorize(Func<string> getNameAction, string remoteAddress);
        void SendCommand(byte[] bytes);

        event Action<byte[]> DataReceived;
        event Action SessionClosed;
    } 
    
    public class TransmitSession : ITransmitSession
    {
        private readonly INetworkConnector _networkConnector;
        private readonly INetworkAddressFactory _networkAddressFactory;
        private readonly IRecorder _recorder;
        
        private TaskCompletionSource<bool> _authorizationResult;
        private AuthorizatorStatus _status;
        private INetworkTunnel _opennedTunnel;
        private Func<string> _getNameAction;
        private bool IsAuthorizated => _status == AuthorizatorStatus.Authorizated;
        
        public event Action<byte[]> DataReceived;
        public event Action SessionClosed;

        public TransmitSession(INetworkConnector networkConnector, INetworkAddressFactory networkAddressFactory, IRecorder recorder)
        {
            _status = AuthorizatorStatus.NOT_Authorizated;
            _recorder = recorder;
            _networkConnector = networkConnector;
            _authorizationResult = new TaskCompletionSource<bool>(false);
            _networkAddressFactory = networkAddressFactory;
        }

        public async Task<bool> Authorize(Func<string> getNameAction, string remoteAddress)
        {
            _getNameAction = getNameAction;
            if (_status == AuthorizatorStatus.Authorizated)
            {
                return true;
            }

            if (_status == AuthorizatorStatus.NOT_Authorizated)
            {
                if (_networkConnector.TryConnect(_networkAddressFactory.Create(remoteAddress),
                    out INetworkTunnel tunnel))
                {
                    _opennedTunnel = tunnel;
                    _opennedTunnel.Closed += TunnelClosedHandler;
                    _opennedTunnel.Received += BytesReceivedHandler;
                    _opennedTunnel.Listen();
                    _recorder.RecordInfo(nameof(TransmitSession), $"NOT_Authorized Send '{_getNameAction()}'");
                    SafeExecution.TryCatch(() => _opennedTunnel.Send(Encoding.ASCII.GetBytes(_getNameAction())), e => ExternalExceptionHandler(e, "Exception in tunnel.Send()"));
                }
                else
                {
                    _recorder.RecordError(nameof(TransmitSession), "Fail connect to remote address");
                    return false;
                }
            }

            await SafeExecution.TryCatchAsync(_authorizationResult.Task, ExceptionHandler);
            return _authorizationResult.Task.Result;
        }

        public void SendCommand(byte[] bytes)
        {
            SafeExecution.TryCatch(() => SendCommandInternal(bytes), e => ExternalExceptionHandler(e, "Exception in SendCommand"));
        }
        
        public void SendCommandInternal(byte[] bytes)
        {
            var wrapped = CommandFilter.WrapDataWithPrefix(bytes, (byte)NodeCommands.COMMAND_PREFIX);
            _opennedTunnel.Send(wrapped);
        }

        private void BytesReceivedHandler(byte[] bytes)
        {
            SafeExecution.TryCatch(() => BytesReceivedHandlerInternal(bytes), ExceptionHandler);
        }
        
        private void BytesReceivedHandlerInternal(byte[] bytes)
        {
            var result = CommandFilter.TryGetInnerData(bytes, (byte)NodeCommands.RESPONSE_PREFIX);

            if (!result.Item1)
            {
                _recorder.RecordError(nameof(TransmitSession), "Wrong command received");
                return;
            }
            
            if (!IsAuthorizated)
            {
                if (result.Item2[0].Equals((byte)NodeCommands.RESPONSE_LOGIN_SUCCESS))
                {
                    _recorder.RecordInfo(nameof(TransmitSession), "Response login success");
                    _status = AuthorizatorStatus.Authorizated;
                    _authorizationResult.SetResult(true);
                    return;
                }
                
                _recorder.RecordError(nameof(TransmitSession), "Command received on not authorize");
                return;
            }
            
            SafeExecution.TryCatch(() => DataReceived?.Invoke(bytes), 
                e => ExternalExceptionHandler(e, "Exception DataReceived Thread"));
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(TransmitSession), exception.Message);
        }
        
        private void ExternalExceptionHandler(Exception exception, string message)
        {
            _recorder.RecordError(nameof(TransmitSession), $"{message} {exception.Message}");
        }

        private void TunnelClosedHandler()
        {
            SessionClosed?.Invoke();
        }

        private enum AuthorizatorStatus
        {
            NOT_Authorizated,
            Authorizated
        }

        public void Dispose()
        {
            _opennedTunnel.Closed -= TunnelClosedHandler;
            _opennedTunnel.Received -= BytesReceivedHandler;
            _opennedTunnel.Dispose();
        }
    }
}