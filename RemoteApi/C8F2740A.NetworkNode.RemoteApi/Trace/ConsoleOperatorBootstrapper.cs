using System;
using System.Threading.Tasks;

namespace RemoteApi.Trace
{
    public interface IConsoleOperatorBootstrapper
    {
        void Start();
        Task<(bool, string)> ExecuteCommand(string command);
        
        event Action<string, string> Connected;
    }
    
    public class ConsoleOperatorBootstrapper : IConsoleOperatorBootstrapper
    {
        private readonly IRemoteApiOperator _remoteApiOperator;
        
        public ConsoleOperatorBootstrapper(IRemoteApiOperator remoteApiOperator)
        {
            _remoteApiOperator = remoteApiOperator;
            _remoteApiOperator.Disconnected += DisconnectedHandler;
        }

        public void Start()
        {
            ConnectToLocal();
        }

        public Task<(bool, string)> ExecuteCommand(string command)
        {
            return _remoteApiOperator.ExecuteCommand(command);
        }

        public event Action<string, string> Connected
        {
            add => _remoteApiOperator.Connected += value;
            remove => _remoteApiOperator.Connected -= value;
        }

        private void ConnectToLocal()
        {
            _remoteApiOperator.ExecuteCommand("connect 127.0.0.1:55555");
        }

        private void DisconnectedHandler()
        {
            ConnectToLocal();
        }
    }
}