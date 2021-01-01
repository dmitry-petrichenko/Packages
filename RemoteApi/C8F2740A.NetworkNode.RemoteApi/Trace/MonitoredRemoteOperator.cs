using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using RemoteApi.Monitor;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IMonitoredRemoteOperator : IDisposable
    {
        void Start();
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
            SafeExecution.TryCatch(() =>
                {
                    _remoteTraceMonitor.SetPrompt(address);
                    _remoteTraceMonitor.ClearTextBox();
                },
                exception => _recorder.DefaultException(this, exception));
        }

        private void TextReceivedHandler(string value)
        {
            SafeExecution.TryCatch(() => _remoteTraceMonitor.DisplayNextMessage(value),
                exception => _recorder.DefaultException(this, exception));
        }

        public void Start()
        {
            SafeExecution.TryCatch(() => _autoLocalConnector.Start(),
                exception => _recorder.DefaultException(this, exception));
        }

        public void Dispose()
        {
            _autoLocalConnector.TextReceived -= TextReceivedHandler;
            _autoLocalConnector.Connected -= ConnectedHandler;
            _remoteTraceMonitor.TextEntered -= TextEnteredHandler;
            _remoteTraceMonitor.Stop();
        }
    }
}