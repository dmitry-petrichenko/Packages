using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace RemoteApi
{
    public interface IRemoteApiOperator
    {
        Task<bool> ExecuteCommand(string command);
        Task<bool> Connect(string address);
        void Disconnect();
        
        event Action<string> InstructionReceived;
    }
    
    public class RemoteApiOperator : IRemoteApiOperator
    {
        public event Action<string> InstructionReceived;
        
        private Dictionary<string, Func<IEnumerable<string>, Task<(bool, string)>>> _commandsMap;
        private IInstructionSenderHolder _instructionSenderHolder;
        private IRecorder _recorder;

        private readonly IInstructionSenderFactory _instructionsSenderFactory;

        public RemoteApiOperator(
            IInstructionSenderHolder instructionSenderHolder,
            IInstructionSenderFactory instructionsSenderFactory,
            IRecorder recorder)
        {
            _instructionSenderHolder = instructionSenderHolder;
            _instructionsSenderFactory = instructionsSenderFactory;
            _recorder = recorder;

            _instructionSenderHolder.InstructionReceived += InstructionReceivedHandler;
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> value)
        {
            InstructionReceived?.Invoke(value.ToText());
            return Enumerable.Empty<byte>();
        }

        public async Task<bool> ExecuteCommand(string command)
        {
            if (_instructionSenderHolder.HasActiveSender)
            {
                return await TrySendInstructionInternal(command);
            }

            return false;
        }

        private async Task<bool> TrySendInstructionInternal(string command)
        {
            var result = await _instructionSenderHolder.TrySendInstruction(command.ToEnumerableByte());

            if (!result.Item1)
            {
                _recorder.RecordError(GetType().Name, "Fail to execute command");
            }

            return result.Item1;
        }

        public async Task<bool> Connect(string address)
        {
            if (!address.IsCorrectIPv4Address())
            {
                _recorder.RecordError(GetType().Name, "Wrong remote address");
                return false;
            }
            
            var instructionsSender = _instructionsSenderFactory.Create(address);
            var result =  await instructionsSender.TrySendInstruction(RemoteApiCommands.TRACE.ToEnumerableByte());
            if (result.Item1)
            {
                _instructionSenderHolder.Set(instructionsSender);
                return true;
            }

            _recorder.RecordError(GetType().Name, "Fail to connect to remote address");
            return false;
        }

        public void Disconnect()
        {
            if (_instructionSenderHolder.HasActiveSender)
            {
                _instructionSenderHolder.Clear();
            }
        }
    }
}