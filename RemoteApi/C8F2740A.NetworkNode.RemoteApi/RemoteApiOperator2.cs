using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Networking.NetworkExtensions;
using C8F2740A.NetworkNode.SessionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace RemoteApi
{
    public interface IRemoteApiOperator2
    {
        Task<(bool, string)> ExecuteCommand(string command);
        
        event Action<string> Connected;
        event Action Disconnected;
    }
    
    public class RemoteApiOperator2 : IRemoteApiOperator2
    {
        private Dictionary<string, Func<IEnumerable<string>, Task<(bool, string)>>> _commandsMap;
        private IInstructionSender _currentInstructionSender;
        private IInstructionSenderHolder _instructionSenderHolder;
        
        private readonly IInstructionSenderFactory _instructionsSenderFactory;

        public RemoteApiOperator2(
            IInstructionSenderHolder instructionSenderHolder,
            IInstructionSenderFactory instructionsSenderFactory)
        {
            _instructionSenderHolder = instructionSenderHolder;
            _instructionsSenderFactory = instructionsSenderFactory;
            
            _commandsMap = new Dictionary<string, Func<IEnumerable<string>, Task<(bool, string)>>>
            {
                {"connect", ConnectHandler}, 
                {"disconnect", DisconnectHandler}
            };
        }

        public async Task<(bool, string)> ExecuteCommand(string command)
        {
            var textCommand = new TextCommand(command);
            
            if (_commandsMap.TryGetValue(textCommand.Command, out Func<IEnumerable<string>, Task<(bool, string)>> handler))
            {
                return await handler.Invoke(textCommand.Parameters);
            }
            
            if (_instructionSenderHolder.HasActiveSender)
            {
                return await TrySendInstructionInternal(command);
            }
            
            return GenerateWrongCommandResult();
        }

        private async Task<(bool, string)> TrySendInstructionInternal(string command)
        {
            var result = await _instructionSenderHolder.TrySendInstruction(command.ToEnumerableByte());
            
            if (!result.Item1)
            {
                return (result.Item1, result.Item2.ToText());
            }

            if (result.Item2.ToText().Equals(RemoteApiCommands.WRONG_COMMAND))
            {
                return GenerateWrongCommandResult();
            }
            
            return (result.Item1, result.Item2.ToText());
        }

        private (bool, string) GenerateWrongCommandResult()
        {
            return (false, "wrong command");
        }

        public event Action<string> Connected;
        public event Action Disconnected;
        
        private async Task<(bool, string)> ConnectHandler(IEnumerable<string> parameters)
        {
            var parametersArr = parameters.ToArray();
            var address = parametersArr.First();
            if (parametersArr.Length != 1 || !address.IsCorrectIPv4Address())
            {
                return (false, "wrong remote address");
            }
            
            var instructionsSender = _instructionsSenderFactory.Create(address);
            var result =  await instructionsSender.TrySendInstruction(RemoteApiCommands.PING.ToEnumerableByte());
            if (result.Item1)
            {
                _currentInstructionSender = instructionsSender;
                _instructionSenderHolder.Set(_currentInstructionSender);
                Connected?.Invoke(address);
                return (true, "success");
            }
            
            return (false, $"fail to connect {address}");
        }
        
        private Task<(bool, string)> DisconnectHandler(IEnumerable<string> _)
        {
            if (_instructionSenderHolder.HasActiveSender)
            {
                _instructionSenderHolder.Clear();
            }
            Disconnected?.Invoke();
            
            return Task.FromResult((true, String.Empty));
        }
    }
}