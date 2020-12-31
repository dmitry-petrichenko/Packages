using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace RemoteApi.Trace
{
    public interface IAutoLocalConnector
    {
        void Start();
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
            _connectParser.Disconnected += DisconnectedHandler;
        }

        private void DisconnectedHandler()
        {
            SafeExecution.TryCatchAsync(() => ConnectToSelfInternal(),
                exception => _recorder.DefaultException(this, exception));
        }

        public void Start()
        {
            ConnectToSelf();
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return _connectParser.ExecuteCommand(command);
        }

        private void InstructionReceivedHandler(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _recorder.RecordError(GetType().Name, "Text value cannot be empty");
                return;
            }
            
            TextReceived?.Invoke(value);
        }
        
        private void ConnectToSelf()
        {
            SafeExecution.TryCatchAsync(() => ConnectToSelfInternal(),
                exception => _recorder.DefaultException(this, exception));
        }
        
        private async Task ConnectToSelfInternal()
        {
            var isConnected = await _connectParser.ExecuteCommand("connect 127.0.0.1:10000");

            if (!isConnected)
            {
                _recorder.RecordError(GetType().Name, "Fail to connect local");
            }
        }
    }
}