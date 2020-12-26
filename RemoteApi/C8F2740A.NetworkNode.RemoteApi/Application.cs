using System.Threading.Tasks;
using RemoteApi.Trace;

namespace RemoteApi
{
    public class Application
    {
        private readonly IAutoLocalConnector _autoLocalConnector;
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        
        public Application(
            IAutoLocalConnector autoLocalConnector,
            IRemoteTraceMonitor remoteTraceMonitor)
        {
            _autoLocalConnector = autoLocalConnector;
            _remoteTraceMonitor = remoteTraceMonitor;

            _autoLocalConnector.TextReceived += TextReceivedHandler;
            _autoLocalConnector.Connected += ConnectedHandler;
            _remoteTraceMonitor.TextEntered += TextEnteredHandler;
        }

        private void TextEnteredHandler(string value)
        {
            _autoLocalConnector.ExecuteCommand(value);
        }

        private void ConnectedHandler(string address)
        {
            _remoteTraceMonitor.SetPrompt(address);
        }

        private void TextReceivedHandler(string value)
        {
            _remoteTraceMonitor.DisplayNextMessage(value);
        }

        public async Task Start()
        {
            //_remoteTraceMonitor.Start();
            var isStarted = await _autoLocalConnector.Start();
        }
    }
}