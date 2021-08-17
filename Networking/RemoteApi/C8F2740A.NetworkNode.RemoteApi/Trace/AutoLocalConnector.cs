using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.RemoteApi.Trace
{
    public interface IAutoLocalConnector
    {
        void Start();
        Task<bool> ExecuteCommand(string command);
        
        event Action<string> TextReceived;
        event Action<string> Connected;
        event Action Finished;
    }
    
    public class AutoLocalConnector : IAutoLocalConnector
    {
        public event Action<string> TextReceived;
        public event Action<string> Connected
        {
            add  => _connectParser.Connected += value; 
            remove  => _connectParser.Connected -= value; 
        }

        public event Action Finished;

        private readonly IConnectParser _connectParser;
        private readonly IRecorder _recorder;
        private readonly string _address;
        
        public AutoLocalConnector(
            IConnectParser connectParser,
            IRecorder recorder,
            string address)
        {
            _connectParser = connectParser;
            _recorder = recorder;
            _address = address;
            
            _connectParser.InstructionReceived += InstructionReceivedHandler;
            _connectParser.Disconnected += DisconnectedHandler;
            _connectParser.Finished += FinishedHandler;
        }
        
        public void Start()
        {
            ConnectToSelf();
        }

        public Task<bool> ExecuteCommand(string command)
        { 
            return _connectParser.ExecuteCommand(command);
        }
        
        private void FinishedHandler()
        {
            Finished?.Invoke();
        }

        private void DisconnectedHandler()
        {
            ConnectToSelf();
        }

        private void InstructionReceivedHandler(string value)
        {
            if (value == default)
            {
                _recorder.RecordError(GetType().Name, "Text value cannot be default");
                return;
            }
            
            TextReceived?.Invoke(value);
        }
        
        private void ConnectToSelf()
        {
            SafeExecution.TryCatchAsync(ConnectToSelfInternal,
                exception => _recorder.DefaultException(this, exception));
        }
        
        private async Task ConnectToSelfInternal()
        {
            var isConnected = await _connectParser.ExecuteCommand($"connect {_address}");

            if (!isConnected)
            {
                _recorder.RecordError(GetType().Name, "Fail to connect local");
            }
        }
    }
}