using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Extensions;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public interface IRemoteApiOperator
    {
        Task<bool> ExecuteCommand(string command);
        Task<bool> Connect(string address);
        void Disconnect();
        
        event Action<string> InstructionReceived;
    }
    
    public class RemoteApiOperator : IRemoteApiOperator, IDisposable
    {
        public event Action<string> InstructionReceived;
        
        private IInstructionSenderHolder _instructionSenderHolder;

        private readonly IInstructionSenderFactory _instructionsSenderFactory;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly IRecorder _recorder;

        public RemoteApiOperator(
            IInstructionSenderHolder instructionSenderHolder,
            IInstructionSenderFactory instructionsSenderFactory,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _instructionSenderHolder = instructionSenderHolder;
            _instructionsSenderFactory = instructionsSenderFactory;
            _applicationRecorder = applicationRecorder;
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
            
            _recorder.RecordError(GetType().Name, "Trying to send instruction without sender");

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
                _applicationRecorder.RecordInfo(GetType().Name, "Wrong ip address format");
                return false;
            }
            
            var instructionsSender = _instructionsSenderFactory.Create(address);
            _instructionSenderHolder.Set(instructionsSender);
            
            var (result, data) =  await _instructionSenderHolder.TrySendInstruction(RemoteApiCommands.TRACE.ToEnumerableByte());
            
            if (!result)
            {
                // вернуться к локал
                //_instructionSenderHolder.Clear();
                _applicationRecorder.RecordInfo(GetType().Name, "Fail to connect to remote");
            }
            
            return result;
        }

        public void Disconnect()
        {
            if (_instructionSenderHolder.HasActiveSender)
            {
                _instructionSenderHolder.Clear();
            }
        }

        public void Dispose()
        {
        }
    }
}