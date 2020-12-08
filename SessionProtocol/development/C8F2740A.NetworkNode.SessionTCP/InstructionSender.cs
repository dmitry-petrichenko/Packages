using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface IInstructionSender : IDisposable
    {
        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
    
    public class InstructionSender : IInstructionSender
    {
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        
        private readonly INetworkAddress _remoteAddress;
        private readonly INodeVisitor _nodeVisitor;
        private readonly IRecorder _recorder;

        private ISession _currentSession;
        private TaskCompletionSource<IEnumerable<byte>> _sendInstructionTask;
        
        public InstructionSender(
            INodeVisitor nodeVisitor, 
            INetworkAddress remoteAddress,
            IRecorder recorder)
        {
            _nodeVisitor = nodeVisitor;
            _remoteAddress = remoteAddress;
            _recorder = recorder;
            _currentSession = default;
        }
        
        public void Dispose()
        {
            
        }
        
        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            return SafeExecution.TryCatchWithResultAsync(TrySendInstructionInternal(instruction), ExceptionHandler);
        }

        private void SubscribeOnSession(ISession session)
        {
            session.Received += SessionReceivedHandler;
            session.Responded += SessionRespondedHandler;
        }

        private void SessionRespondedHandler(IEnumerable<byte> value)
        {
            _sendInstructionTask.SetResult(value);
        }

        private void SessionReceivedHandler(IEnumerable<byte> obj)
        {
            
        }
        
        private async Task<(bool, IEnumerable<byte>)> TrySendInstructionInternal(IEnumerable<byte> instruction)
        {
            ValidateSend();
            _sendInstructionTask = new TaskCompletionSource<IEnumerable<byte>>();
            
            if (_currentSession != default)
            {
                _currentSession.Send(instruction);
            }
            else
            {
                var connectResult = _nodeVisitor.TryConnect(_remoteAddress);
                if (connectResult.Item1)
                {
                    _currentSession = connectResult.Item2;
                    _currentSession.Send(instruction);
                }
                else
                {
                    return (false, default);
                }
            }

            var response =  await _sendInstructionTask.Task;
            
            return (true, response);
        }

        private void ValidateSend()
        {
            if (_sendInstructionTask != default && !_sendInstructionTask.Task.IsCompleted)
            {
                throw new Exception("Attempt to send white task is not completed");
            }
        }
        
        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(GetType().Name, exception.Message);
        }
    }
}