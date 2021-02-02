using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Monitor;

namespace C8F2740A.NetworkNode.RemoteApi.Trace
{
    public interface IMonitoredRemoteOperator : IDisposable
    {
        void Start();
    }
    
    public class MonitoredRemoteOperator : IMonitoredRemoteOperator
    {
        private readonly IAutoLocalConnector _autoLocalConnector;
        private readonly IRemoteTraceMonitorСonsistent _remoteTraceMonitorСonsistent;
        private readonly IRecorder _recorder;

        public MonitoredRemoteOperator(
            IAutoLocalConnector autoLocalConnector,
            IRemoteTraceMonitorСonsistent remoteTraceMonitorСonsistent,
            IRecorder recorder)
        {
            _autoLocalConnector = autoLocalConnector;
            _remoteTraceMonitorСonsistent = remoteTraceMonitorСonsistent;
            _recorder = recorder;

            _autoLocalConnector.TextReceived += TextReceivedHandler;
            _autoLocalConnector.Connected += ConnectedHandler;
            _remoteTraceMonitorСonsistent.CommandReceived += TextEnteredHandler;
        }

        private Task<bool> TextEnteredHandler(string value)
        {
            return SafeExecution.TryCatchWithResultAsync(() => _autoLocalConnector.ExecuteCommand(value),
                exception => _recorder.DefaultException(this, exception));;
        }

        private void ConnectedHandler(string address)
        {
            SafeExecution.TryCatch(() =>
                {
                    _remoteTraceMonitorСonsistent.SetPrompt(address);
                    _remoteTraceMonitorСonsistent.ClearTextBox();
                },
                exception => _recorder.DefaultException(this, exception));
        }

        private void TextReceivedHandler(string value)
        {
            SafeExecution.TryCatch(() => _remoteTraceMonitorСonsistent.DisplayNextMessage(value),
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
            _remoteTraceMonitorСonsistent.CommandReceived -= TextEnteredHandler;
            _remoteTraceMonitorСonsistent.Stop();
        }
    }
}