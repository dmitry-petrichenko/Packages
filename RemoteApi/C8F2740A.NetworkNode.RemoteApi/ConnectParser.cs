using System;
using System.Linq;
using System.Threading.Tasks;
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
            var commandData = command.ParseToTextCommand();
            return commandData.Command switch
            {
                "connect" => Connect(commandData.Parameters.First()),
                "disconnect" => Disconnect(),
                _ => _remoteApiOperator.ExecuteCommand(command)
            };
        }

        private async Task<bool> Connect(string address)
        {
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