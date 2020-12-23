using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived
        {
            add => _sessionHolder.InstructionReceived += value;
            remove => _sessionHolder.InstructionReceived -= value;
        }
        
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
        }

        public void Dispose()
        {
            if (_sessionHolder.HasActiveSession)
            {
                _sessionHolder.Clear();
            }
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

            var connectResult = _nodeVisitor.TryConnect(_remoteAddress);
            if (connectResult.Item1)
            {
                _sessionHolder.Set(connectResult.Item2);
                return _sessionHolder.SendInstruction(instruction);
            }

            return Task.FromResult((false, Enumerable.Empty<byte>()));
        }
    }
}