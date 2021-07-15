using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface IInstructionSender
    {
        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        bool TryConnect();
        void Dispose();
        
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        event Action Disconnected;
    }
}