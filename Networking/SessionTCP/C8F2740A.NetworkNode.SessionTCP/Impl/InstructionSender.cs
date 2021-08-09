using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP.Impl
{
    public class InstructionSender : IInstructionSender
    {
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived
        {
            add => _sessionHolder.InstructionReceived += value;
            remove => _sessionHolder.InstructionReceived -= value;
        }

        public event Action Disconnected;

        private readonly INetworkAddress _remoteAddress;
        private readonly INodeVisitor _nodeVisitor;
        private readonly IRecorder _recorder;

        private ISessionHolder _sessionHolder;

        public InstructionSender(
            INodeVisitor nodeVisitor, 
            INetworkAddress remoteAddress,
            ISessionHolder sessionHolder,
            IRecorder recorder)
        {
            _nodeVisitor = nodeVisitor;
            _remoteAddress = remoteAddress;
            _recorder = recorder;
            _sessionHolder = sessionHolder;

            _sessionHolder.Disconnected += SessionDisconnectedHandler;
        }

        private void SessionDisconnectedHandler()
        {
            _sessionHolder.Clear();
            
            if (!TryConnectAndResetSessionHolder())
            {
                Disconnected?.Invoke();
            }
        }

        public void Dispose()
        {
            _sessionHolder.Disconnected -= SessionDisconnectedHandler;
            
            if (_sessionHolder.HasActiveSession)
            {
                _sessionHolder.Clear();
            }
        }
        
        public bool TryConnect()
        {
            return TryConnectAndResetSessionHolder();
        }
        
        public async Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            var result = (false, Enumerable.Empty<byte>());
            try
            {
                result = await TrySendInstructionInternal(instruction);
            }
            catch (Exception exception)
            {
                _recorder.DefaultException(this, exception);
            }

            return result;
        }

        private Task<(bool, IEnumerable<byte>)> TrySendInstructionInternal(IEnumerable<byte> instruction)
        {
            if (_sessionHolder.HasActiveSession)
            {
                return _sessionHolder.SendInstruction(instruction);
            }
            
            _recorder.RecordError(GetType().Name, "Trying to sent instruction without session");
            
            return Task.FromResult((false, Enumerable.Empty<byte>()));
        }

        private bool TryConnectAndResetSessionHolder()
        {
            var (isConnected, session) = _nodeVisitor.TryConnect(_remoteAddress);
            if (isConnected)
            {
                _sessionHolder.Set(session);
                return true;
            }

            return false;
        }
    }
}