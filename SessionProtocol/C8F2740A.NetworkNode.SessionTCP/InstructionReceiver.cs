using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ISessionHolder _sessionHolder;
        
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived
        {
            add => _sessionHolder.InstructionReceived += value;
            remove => _sessionHolder.InstructionReceived -= value;
        }
        
        public InstructionReceiver(
            INodeGateway nodeGateway, 
            ISessionHolder sessionHolder,
            IRecorder recorder)
        {
            _recorder = recorder;
            _nodeGateway = nodeGateway;
            _sessionHolder = sessionHolder;

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

        public void Dispose()
        {
            _sessionHolder.Clear();
            _nodeGateway.ConnectionReceived -= ConnectionReceivedHandler;
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            if (_sessionHolder.HasActiveSession)
            {
                return _sessionHolder.SendInstruction(instruction);
            }
            
            _recorder.RecordError(GetType().Name, "Trying to sent instruction without session");
            return Task.FromResult((false, Enumerable.Empty<byte>()));
        }
    }
}