using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface IInstructionReceiver
    {
        bool HasActiveSession { get; }

        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
}