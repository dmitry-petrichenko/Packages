using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Extensions;
using C8F2740A.NetworkNode.SessionTCP;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public interface ITextToRemoteSender
    {
        Task<bool> TrySendText(string instruction);
    }
    
    public interface IRemoteApiMap
    {
        void RegisterWrongCommandHandler(Action action);
        void RegisterCommand(string name, Action handler, string description = "");
        void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "");
    }
    
    public class RemoteApiMap : IRemoteApiMap, ITextToRemoteSender
    {
        private readonly IInstructionReceiver _instructionsReceiver;
        private readonly IRecorder _recorder;

        private Action WrongCommandHandler { get; set;  }
        
        private Dictionary<string, bool> _commandWithParametersMap;
        private Dictionary<string, Action<IEnumerable<string>>> _commandHandlerMap;
        private Dictionary<string, string> _commandDescriptionMap;

        public RemoteApiMap(
            IInstructionReceiver instructionsReceiver,
            IRecorder recorder)
        {
            _commandWithParametersMap = new Dictionary<string, bool>();
            _commandHandlerMap = new Dictionary<string, Action<IEnumerable<string>>>();
            _commandDescriptionMap = new Dictionary<string, string>();
            
            _instructionsReceiver = instructionsReceiver;
            _recorder = recorder;
            
            _instructionsReceiver.InstructionReceived += CommandHandler;
        }

        public void RegisterWrongCommandHandler(Action action)
        {
            WrongCommandHandler = action;
        }

        public void RegisterCommand(string name, Action handler, string description = "")
        {
            _commandWithParametersMap.Add(name, false);
            _commandHandlerMap.Add(name, s => handler.Invoke());
            _commandDescriptionMap.Add(name, description);
        }

        public void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "")
        {
            _commandWithParametersMap.Add(name, true);
            _commandHandlerMap.Add(name, s => handler.Invoke(s));
            _commandDescriptionMap.Add(name, description);
        }
        
        private IEnumerable<byte> CommandHandler(IEnumerable<byte> received)
        {
            SafeExecution.TryCatch(() => CommandHandlerInternal(received),
                exception => _recorder.DefaultException(this, exception));
            
            return Enumerable.Empty<byte>();
        }
        
        private void CommandHandlerInternal(IEnumerable<byte> received)
        {
            var commandAndParameters = ExtractCommandWidthParameters(received);
            if (_commandWithParametersMap.TryGetValue(commandAndParameters.Item1, out bool withParameter))
            {
                if (withParameter)
                {
                    _commandHandlerMap[commandAndParameters.Item1].Invoke(commandAndParameters.Item2);
                    return;
                }

                _commandHandlerMap[commandAndParameters.Item1].Invoke(Enumerable.Empty<string>());
                return;
            }
            
            WrongCommandHandler?.Invoke();
        }

        private (string, IEnumerable<string>) ExtractCommandWidthParameters(IEnumerable<byte> received)
        {
            var result = Encoding.UTF8.GetString(received.ToArray());
            var array = result.Split(" ");
            
            return (array.First(), array.Skip(1).ToArray());
        }

        public async Task<bool> TrySendText(string instruction)
        {
            var result = await _instructionsReceiver.TrySendInstruction(instruction.ToEnumerableByte());

            if (!result.Item1)
            {
                _recorder?.RecordError(GetType().Name, "Fail to send text");
            }

            return result.Item1;
        }
    }
}