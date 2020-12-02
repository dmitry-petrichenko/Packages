using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.Commands;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface INodeVisitor : IDisposable
    {
        Task<(bool, byte[])> TrySendCommand(byte[] bytes, string password);
    }

    public class NodeVisitor : INodeVisitor
    {
        private readonly ITransmitSessionFactory _transmitSessionFactory;
        private readonly string _remoteAddress;

        private ITransmitSession _currentSession;
        private IRecorder _recorder;
        private TaskCompletionSource<byte[]> _sendCommandTask;
        
        public NodeVisitor(string remoteAddress, ITransmitSessionFactory transmitSessionFactory, IRecorder recorder)
        {
            _remoteAddress = remoteAddress;
            _transmitSessionFactory = transmitSessionFactory;
            _recorder = recorder;
        }

        public void Dispose()
        {
            _currentSession.SessionClosed -= SessionClosedHandler;
            _currentSession.DataReceived -= BytesReceivedHandler;
            _currentSession.Dispose();
            _currentSession = default;
        }

        public Task<(bool, byte[])> TrySendCommand(byte[] bytes, string password)
        {
            return SafeExecution.TryCatchWithResultAsync(TrySendCommandInternal(bytes, password), ExceptionHandler);
        }

        private async Task<(bool, byte[])> TrySendCommandInternal(byte[] bytes, string password)
        {
            if (_currentSession == default)
            {
                _currentSession = _transmitSessionFactory.Create();
                _currentSession.SessionClosed += SessionClosedHandler;
                _currentSession.DataReceived += BytesReceivedHandler;

                var authorizeResult = await _currentSession.Authorize(() => password, _remoteAddress);

                if (!authorizeResult)
                {
                    _recorder.RecordError(nameof(NodeVisitor), "Authorization fail");
                    return (false, Array.Empty<byte>());
                }
            }
            
            var result = await SendCommandInternal(bytes);
            return (true, result);
        }

        private Task<byte[]> SendCommandInternal(byte[] bytes)
        {
            if (_sendCommandTask != null && !_sendCommandTask.Task.IsCompleted)
            {
                _sendCommandTask.SetResult(Array.Empty<byte>());
            }
            _sendCommandTask = new TaskCompletionSource<byte[]>();
            _currentSession.SendCommand(bytes);

            return _sendCommandTask.Task;
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(NodeVisitor), exception.Message);
        }
        
        private void BytesReceivedHandler(byte[] bytes)
        {
            var belong = CommandFilter.IsCommandBelongToPrefix((NodeCommands) bytes[0], NodeCommands.RESPONSE_PREFIX);
            if (!belong)
            {
                _recorder.RecordError(nameof(InstructionsSender), "Response do not belong to protocol");
                _sendCommandTask?.SetResult(Array.Empty<byte>());
            }
            else
            {
                _sendCommandTask?.SetResult(bytes.Skip(1).ToArray());
            }
        }

        private void SessionClosedHandler()
        {
            _currentSession.SessionClosed -= SessionClosedHandler;
            _currentSession.DataReceived -= BytesReceivedHandler;
            _currentSession.Dispose();
            _currentSession = default;
        }
    }
}