using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.SessionTCP.Impl
{
    public class InstructionReceiver : IInstructionReceiver
    {
        private readonly IRecorder _recorder;
        private readonly INodeGateway _nodeGateway;
        private readonly ISessionHolder _sessionHolder;
        
        public bool HasActiveSession => _sessionHolder.HasActiveSession;
        
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
            _sessionHolder.Disconnected += DisconnectedHandler;
        }

        private void DisconnectedHandler()
        {
            ClearSession();
        }

        private void ClearSession()
        {
            if (_sessionHolder.HasActiveSession)
            {
                _sessionHolder.Clear();
            }
        }

        private void ConnectionReceivedHandler(ISession session)
        {
            ClearSession();
            
            _sessionHolder.Set(session);
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