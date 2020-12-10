using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface IInstructionReceiver : IDisposable
    {
        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }

    public class InstructionReceiver : IInstructionReceiver
    {
        private readonly IRecorder _recorder;
        private readonly INodeGateway _nodeGateway;
        
        private ISessionHolder _sessionHolder;
        
        public InstructionReceiver(INodeGateway nodeGateway, IRecorder recorder)
        {
            _recorder = recorder;
            _nodeGateway = nodeGateway;
            _sessionHolder = new SessionHolder();
            _sessionHolder.InstructionReceived += InstructionReceivedHandler;

            _nodeGateway.ConnectionReceived += ConnectionReceivedHandler;
        }

        private void ConnectionReceivedHandler(ISession session)
        {
            if (_sessionHolder.HasActiveSession)
            {
                _sessionHolder.Clear();
            }
            
            _sessionHolder.Set(session);
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> value)
        {
            return InstructionReceived?.Invoke(value);
        }

        public void Dispose()
        {
            _sessionHolder.Clear();
            _sessionHolder.InstructionReceived -= InstructionReceivedHandler;
            _nodeGateway.ConnectionReceived -= ConnectionReceivedHandler;
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            if (_sessionHolder.HasActiveSession)
            {
                return _sessionHolder.SendInstruction(instruction);
            }
            
            _recorder.RecordError(GetType().Name, "Trying to sent instruction without session");
            return Task.FromResult<(bool, IEnumerable<byte>)>((false, default));
        }

        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
}