using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface IInstructionReceiver : IDisposable
    {
        bool HasActiveSession { get; }

        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
}