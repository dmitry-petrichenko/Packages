using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public interface IInstructionSenderHolder
    {
        void Set(IInstructionSender instructionSender);
        void Clear();
        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);

        bool HasActiveSender { get; }

        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        event Action Disconnected;
    } 
    
    public class InstructionSenderHolder : IInstructionSenderHolder
    {
        public bool HasActiveSender { get; private set; }
        
        private readonly IRecorder _recorder;
        
        private IInstructionSender _currentInstructionSender;
        
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        public event Action Disconnected;
        
        public InstructionSenderHolder(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public void Set(IInstructionSender instructionSender)
        {
            if (HasActiveSender)
            {
                Clear();
            }
            
            _currentInstructionSender = instructionSender;
            _currentInstructionSender.InstructionReceived += InstructionReceivedHandler;
            _currentInstructionSender.Disconnected += DisconnectedHandler;
            HasActiveSender = true;
        }
        
        private void DisconnectedHandler()
        {
            Disconnected?.Invoke();
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> instruction)
        {
            return InstructionReceived?.Invoke(instruction);
        }

        public void Clear()
        {
            _currentInstructionSender.InstructionReceived -= InstructionReceivedHandler;
            _currentInstructionSender.Disconnected -= DisconnectedHandler;
            _currentInstructionSender.Dispose();
            _currentInstructionSender = default;
            HasActiveSender = false;
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            return _currentInstructionSender.TrySendInstruction(instruction);
        }
    }
}