using System;
using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace RemoteApi.Trace
{
    public interface IAutoLocalConnector
    {
        Task<bool> Start();
        Task<bool> ExecuteCommand(string command);
        
        event Action<string> TextReceived;
        event Action<string> Connected;
    }
    
    public class AutoLocalConnector : IAutoLocalConnector
    {
        public event Action<string> TextReceived;
        public event Action<string> Connected
        {
            add  => _connectParser.Connected += value; 
            remove  => _connectParser.Connected -= value; 
        }

        private readonly IConnectParser _connectParser;
        private readonly IRecorder _recorder;
        
        public AutoLocalConnector(
            IConnectParser connectParser,
            IRecorder recorder)
        {
            _connectParser = connectParser;
            _recorder = recorder;
            
            _connectParser.InstructionReceived += InstructionReceivedHandler;
        }

        private void InstructionReceivedHandler(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _recorder.RecordError(GetType().Name, "Text value cannot be empty");
            }
            
            TextReceived?.Invoke(value);
        }

        public async Task<bool> Start()
        {
            var isConnected = await _connectParser.ExecuteCommand("connect 127.0.0.1:10000");

            if (!isConnected)
            {
                _recorder.RecordError(GetType().Name, "Fail to connect local");
            }

            return isConnected;
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return _connectParser.ExecuteCommand(command);
        }
    }
}