using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Extensions;

namespace C8F2740A.NetworkNode.RemoteApi.Trace
{
    public interface IConnectParser
    {
        Task<bool> ExecuteCommand(string command);

        event Action<string> InstructionReceived;
        event Action<string> Connected;
        event Action Disconnected;
        event Action Finished;
    }
    
    public class ConnectParser : IConnectParser
    {
        public event Action<string> InstructionReceived
        {
            add => _remoteApiOperator.InstructionReceived += value; 
            remove => _remoteApiOperator.InstructionReceived -= value; 
        }

        public event Action Disconnected;
        public event Action Finished;
        public event Action<string> Connected;
        
        private readonly IRemoteApiOperator _remoteApiOperator;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly IRecorder _recorder;
        
        public ConnectParser(
            IRemoteApiOperator remoteApiOperator,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _remoteApiOperator = remoteApiOperator;
            _applicationRecorder = applicationRecorder;
            _recorder = recorder;

            _remoteApiOperator.Disconnected += DisconnectedHandler;
        }

        private void DisconnectedHandler()
        {
            Disconnected?.Invoke();
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
                "exit" => Exit(),
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
                _applicationRecorder.RecordInfo(GetType().Name, "address cannot be empty");
                return false;
            }

            address = SubstituteAbbreviation(address);
            
            var connected = await _remoteApiOperator.Connect(address);
            if (connected)
            {
                Connected?.Invoke(address);
                return true;
            }

            return false;
        }

        private string SubstituteAbbreviation(string address)
        {
            return address.Replace("l:", "127.0.0.1:");
        }
        
        private Task<bool> Disconnect()
        {
            _remoteApiOperator.Disconnect();
            Disconnected?.Invoke();
            
            return Task.FromResult(true);
        }
        
        private Task<bool> Exit()
        {
            Finished?.Invoke();
            
            return Task.FromResult(true);
        }
    }
}