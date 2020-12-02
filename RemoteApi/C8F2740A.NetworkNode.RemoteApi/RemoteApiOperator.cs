using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Networking.NetworkExtensions;
using C8F2740A.NetworkNode.SessionProtocol;
using RemoteApiCommands;

namespace RemoteApi
{
    public interface IRemoteApiOperator
    {
        Task<(bool, IEnumerable<byte>)> ExecuteCommand(string command);
        
        event Action<string> ConnectedSuccess;
        event Action<string> Error;
    }
    
    public class RemoteApiOperator : IRemoteApiOperator
    {
        private readonly IInstructionsSenderFactory _instructionsSenderFactory;
        
        private Dictionary<string, Func<IEnumerable<string>, Task>> _commandsMap;
        private IInstructionsSender _currentInstructionSender;

        public RemoteApiOperator(IInstructionsSenderFactory instructionsSenderFactory)
        {
            _instructionsSenderFactory = instructionsSenderFactory;
            
            _commandsMap = new Dictionary<string, Func<IEnumerable<string>, Task>>
            {
                {"connect", ConnectHandler}, 
                {"disconnect", DisconnectHandler}
            };
        }
        
        public async Task<(bool, IEnumerable<byte>)> ExecuteCommand(string command)
        {
            var textCommand = new TextCommand(command);
            
            if (_commandsMap.TryGetValue(textCommand.Command, out Func<IEnumerable<string>, Task> handler))
            {
                await handler.Invoke(textCommand.Parameters);
                return (true, Enumerable.Empty<byte>());
            }

            if (_currentInstructionSender != default)
            {
                var result = await _currentInstructionSender.TrySendInstruction(command.ToBytesArray());
                return (result.Item1, result.Item2);
            }
            
            Error?.Invoke("wrong command");
            return (true, Enumerable.Empty<byte>());
        }

        private async Task ConnectHandler(IEnumerable<string> parameters)
        {
            var parametersArr = parameters.ToArray();
            if (parametersArr.Length != 1 || !parametersArr.First().IsCorrectIPv4Address())
            {
                Error?.Invoke("wrong remote address");
                return;
            }
            
            var instructionsSender = _instructionsSenderFactory.Create(parametersArr.First());
            var result =  await instructionsSender.TrySendInstruction(((byte)Commands.PING).ToBytesArray());
            if (result.Item1)
            {
                ConnectedSuccess?.Invoke(parametersArr.First());
                _currentInstructionSender = instructionsSender;
            }
        }
        
        private Task DisconnectHandler(IEnumerable<string> _)
        {
            throw new NotImplementedException();
        }

        public event Action<string> ConnectedSuccess;
        public event Action<string> Error;
    }
}