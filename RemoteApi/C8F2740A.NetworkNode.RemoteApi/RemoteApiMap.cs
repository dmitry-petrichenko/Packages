using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IRemoteApiMap
    {
        event Func<IEnumerable<byte>> Connected;

        Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction);
        
        void RegisterCommand(string name, Func<IEnumerable<byte>> handler, string description = "");
        void RegisterCommandWithParameters(string name, Func<IEnumerable<string>, IEnumerable<byte>> handler, string description = "");
    }
    
    public class RemoteApiMap : IRemoteApiMap
    {
        private readonly IInstructionReceiver _instructionsReceiver;

        private Dictionary<string, bool> _commandWithParametersMap;
        private Dictionary<string, Func<IEnumerable<string>, IEnumerable<byte>>> _commandHandlerMap;
        private Dictionary<string, string> _commandDescriptionMap;

        public event Func<IEnumerable<byte>> Connected;
        
        public RemoteApiMap(IInstructionReceiver instructionsReceiver)
        {
            _commandWithParametersMap = new Dictionary<string, bool>();
            _commandHandlerMap = new Dictionary<string, Func<IEnumerable<string>, IEnumerable<byte>>>();
            _commandDescriptionMap = new Dictionary<string, string>();
            
            _instructionsReceiver = instructionsReceiver;
            _instructionsReceiver.InstructionReceived += CommandHandler;
            
            RegisterDefaultCommands();
        }

        public void RegisterCommand(string name, Func<IEnumerable<byte>> handler, string description = "")
        {
            _commandWithParametersMap.Add(name, false);
            _commandHandlerMap.Add(name, s => handler.Invoke());
            _commandDescriptionMap.Add(name, description);
        }

        public void RegisterCommandWithParameters(string name, Func<IEnumerable<string>, IEnumerable<byte>> handler, string description = "")
        {
            _commandWithParametersMap.Add(name, true);
            _commandHandlerMap.Add(name, s => handler.Invoke(s));
            _commandDescriptionMap.Add(name, description);
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand("capacity", ShowCapacity);
        }

        private IEnumerable<byte> ShowCapacity()
        {
            var result = string.Empty;
            foreach (var kvp in _commandDescriptionMap)
            {
                result += $"{kvp.Key} {kvp.Value} {Environment.NewLine}";
            }

            return result.ToEnumerableByte();
        }

        private IEnumerable<byte> CommandHandler(IEnumerable<byte> received)
        {
            if (received.ToText().Equals(RemoteApiCommands.TRACE))
            {
                return Connected?.Invoke();
            }
            
            var commandAndParameters = ExtractCommandWidthParameters(received);
            if (_commandWithParametersMap.TryGetValue(commandAndParameters.Item1, out bool withParameter))
            {
                if (withParameter)
                {
                    return _commandHandlerMap[commandAndParameters.Item1].Invoke(commandAndParameters.Item2);
                }

                return _commandHandlerMap[commandAndParameters.Item1].Invoke(Enumerable.Empty<string>());
            }
            
            return RemoteApiCommands.WRONG_COMMAND.ToEnumerableByte();
        }

        private (string, IEnumerable<string>) ExtractCommandWidthParameters(IEnumerable<byte> received)
        {
            var result = Encoding.UTF8.GetString(received.ToArray());
            var array = result.Split(" ");
            
            return (array.First(), array.Skip(1).ToArray());
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            return _instructionsReceiver.TrySendInstruction(instruction);
        }
    }
}