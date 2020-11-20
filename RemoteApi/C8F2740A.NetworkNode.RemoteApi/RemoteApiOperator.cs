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
        void Start();
        void Stop();
    }
    
    public class RemoteApiOperator : IRemoteApiOperator
    {
        private readonly IInstructionsSenderFactory _instructionsSenderFactory;
        
        private Dictionary<string, Func<IEnumerable<string>, Task>> _commandsMap;
        private IConsolePromptChat _consolePromptChat;
        private IInstructionsSender _currentInstructionSender;

        public RemoteApiOperator(IInstructionsSenderFactory instructionsSenderFactory)
        {
            _instructionsSenderFactory = instructionsSenderFactory;
            
            _commandsMap = new Dictionary<string, Func<IEnumerable<string>, Task>>
            {
                {"connect", ConnectHandler}, 
                {"disconnect", DisconnectHandler}
            };
            
            _consolePromptChat = new ConsolePromptChat();
            _consolePromptChat.CommandReceived += CommandReceivedHandler;
        }

        private async Task CommandReceivedHandler(string command)
        {
            var textCommand = new TextCommand(command);
            
            if(_commandsMap.TryGetValue(textCommand.Command, out Func<IEnumerable<string>, Task> handler))
            {
                await handler.Invoke(textCommand.Parameters);
            }
            else
            {
                if (_currentInstructionSender != default)
                {
                    var result = await _currentInstructionSender.TrySendInstruction(command.ToBytesArray());
                    if (result.Item1)
                    {
                        Console.WriteLine(result.Item2.ToText());
                    }
                }
                else
                {
                    Console.WriteLine("wrong command");
                }
            }
        }

        private async Task ConnectHandler(IEnumerable<string> parameters)
        {
            var parametersArr = parameters.ToArray();
            if (parametersArr.Length != 1 || !parametersArr.First().IsCorrectIPv4Address())
            {
                Console.WriteLine("wrong remote address");
                return;
            }
            
            var instructionsSender = _instructionsSenderFactory.Create(parametersArr.First());
            var result =  await instructionsSender.TrySendInstruction(((byte)Commands.PING).ToBytesArray());
            if (result.Item1)
            {
                _consolePromptChat.Prompt = parametersArr.First();
                _currentInstructionSender = instructionsSender;
            }
        }
        
        private Task DisconnectHandler(IEnumerable<string> _)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            _consolePromptChat.ReadFromInput();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}