using System;
using System.Collections.Generic;
using System.Linq;
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

        private ISessionHolder _sessionHolder;

        public InstructionSender(
            INodeVisitor nodeVisitor, 
            INetworkAddress remoteAddress,
            IRecorder recorder)
        {
            _nodeVisitor = nodeVisitor;
            _remoteAddress = remoteAddress;
            _recorder = recorder;
            _sessionHolder = new SessionHolder(_recorder);
            _sessionHolder.InstructionReceived += InstructionReceivedHandler;
        }

        public void Dispose()
        {
            if (_sessionHolder.HasActiveSession)
            {
                _sessionHolder.Clear();
            }
        }
        
        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            return SafeExecution.TryCatchWithResultAsync(TrySendInstructionInternal(instruction), ExceptionHandler);
        }

        private Task<(bool, IEnumerable<byte>)> TrySendInstructionInternal(IEnumerable<byte> instruction)
        {
            if (_sessionHolder.HasActiveSession)
            {
                return _sessionHolder.SendInstruction(instruction);
            }

            var connectResult = _nodeVisitor.TryConnect(_remoteAddress);
            if (connectResult.Item1)
            {
                _sessionHolder.Set(connectResult.Item2);
                return _sessionHolder.SendInstruction(instruction);
            }

            return Task.FromResult((false, Enumerable.Empty<byte>()));
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(GetType().Name, exception.Message);
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> value)
        {
            return InstructionReceived?.Invoke(value);
        }
    }
}