using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public interface IInstructionSenderReplacer
    {
        void SetTemp(IInstructionSender instructionSender);
        void ResetTemp();
        void SetPermanent(IInstructionSender instructionSender);
        void Clear();
        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);

        bool HasActiveSender { get; }

        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
    
    public class InstructionSenderReplacer : IInstructionSenderReplacer
    { 
        private IInstructionSender _instructionSenderTemp;
        private IInstructionSender _instructionSenderPermanent;
        private IInstructionSenderHolder _instructionSenderHolder;
        private bool TempActive { get; set; }

        public InstructionSenderReplacer(IInstructionSenderHolder instructionSenderHolder)
        {
            _instructionSenderTemp = default;
            _instructionSenderPermanent = default;
            _instructionSenderHolder = instructionSenderHolder;
        }

        public void SetTemp(IInstructionSender instructionSender)
        {
            TempActive = true;
            
            _instructionSenderTemp = instructionSender;
            _instructionSenderTemp.InstructionReceived += InstructionReceivedHandler;

            if (_instructionSenderPermanent != null)
            {
                _instructionSenderPermanent.InstructionReceived -= InstructionReceivedHandler;
            }

            TempActive = true;
        }

        public void ResetTemp()
        {
            TempActive = false;
            
            if (_instructionSenderTemp != null)
            {
                _instructionSenderTemp.InstructionReceived -= InstructionReceivedHandler;
            }

            _instructionSenderTemp = default;
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> data)
        {
            return InstructionReceived?.Invoke(data);
        }

        public void SetPermanent(IInstructionSender instructionSender)
        {
            TempActive = false; 
            
            _instructionSenderPermanent = instructionSender;
            _instructionSenderHolder.Set(instructionSender);
            
            ResetTemp();
        }

        public void Clear()
        {
            TempActive = false;
            _instructionSenderHolder.Clear();
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            if (TempActive)
            {
                return _instructionSenderTemp?.TrySendInstruction(instruction);
            }
            
            return _instructionSenderPermanent?.TrySendInstruction(instruction);
        }

        public bool HasActiveSender => _instructionSenderHolder.HasActiveSender;
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
}