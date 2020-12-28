using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace RemoteApi
{
    public interface IConnectParser
    {
        Task<bool> ExecuteCommand(string command);

        event Action<string> InstructionReceived;
        event Action<string> Connected;
    }
    
    public class ConnectParser : IConnectParser
    {
        public event Action<string> InstructionReceived
        {
            add => _remoteApiOperator.InstructionReceived += value; 
            remove => _remoteApiOperator.InstructionReceived -= value; 
        }

        public event Action<string> Connected;
        
        private readonly IRemoteApiOperator _remoteApiOperator;
        private readonly IRecorder _recorder;
        
        public ConnectParser(
            IRemoteApiOperator remoteApiOperator,
            IRecorder recorder)
        {
            _remoteApiOperator = remoteApiOperator;
            _recorder = recorder;
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return SafeExecution.TryCatchWithResultAsync(() => ExecuteCommandInternal(command),
                exception => _recorder.DefaultException(this, exception));
        }
        
        public Task<bool> ExecuteCommandInternal(string command)
        {
            var commandData = command.ParseToTextCommand();
            return commandData.Command switch
            {
                "connect" => Connect(commandData.Parameters.FirstOrDefault()),
                "disconnect" => Disconnect(),
                _ => _remoteApiOperator.ExecuteCommand(command)
            };
        }

        private Task<bool> Connect(string address)
        {
            return SafeExecution.TryCatchWithResultAsync(() => ConnectInternal(address),
                exception => _recorder.DefaultException(this, exception));
        }
        
        private async Task<bool> ConnectInternal(string address)
        {
            if (address == default)
            {
                _recorder.DefaultException(this, new Exception("Address cannot be null"));
                return false;
            }
            
            var connected = await _remoteApiOperator.Connect(address);
            if (connected)
            {
                Connected?.Invoke(address);
                return true;
            }

            return false;
        }
        
        private Task<bool> Disconnect()
        {
            _remoteApiOperator.Disconnect();
            return Task.FromResult(true);
        }
    }
}