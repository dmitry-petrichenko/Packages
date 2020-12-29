using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using RemoteApi.Monitor;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IMonitoredRemoteOperator
    {
        Task Start();
    }
    
    public class MonitoredRemoteOperator : IMonitoredRemoteOperator
    {
        private readonly IAutoLocalConnector _autoLocalConnector;
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        private readonly IRecorder _recorder;
        
        public MonitoredRemoteOperator(
            IAutoLocalConnector autoLocalConnector,
            IRemoteTraceMonitor remoteTraceMonitor,
            IRecorder recorder)
        {
            _autoLocalConnector = autoLocalConnector;
            _remoteTraceMonitor = remoteTraceMonitor;
            _recorder = recorder;

            _autoLocalConnector.TextReceived += TextReceivedHandler;
            _autoLocalConnector.Connected += ConnectedHandler;
            _remoteTraceMonitor.TextEntered += TextEnteredHandler;
        }

        private void TextEnteredHandler(string value)
        {
            SafeExecution.TryCatch(() => _autoLocalConnector.ExecuteCommand(value),
                exception => _recorder.DefaultException(this, exception));
        }

        private void ConnectedHandler(string address)
        {
            SafeExecution.TryCatch(() => _remoteTraceMonitor.SetPrompt(address),
                exception => _recorder.DefaultException(this, exception));
        }

        private void TextReceivedHandler(string value)
        {
            SafeExecution.TryCatch(() => _remoteTraceMonitor.DisplayNextMessage(value),
                exception => _recorder.DefaultException(this, exception));
        }

        public Task Start()
        {
            return SafeExecution.TryCatchAsync(() => _autoLocalConnector.Start(),
                exception => _recorder.DefaultException(this, exception));
        }
    }
}