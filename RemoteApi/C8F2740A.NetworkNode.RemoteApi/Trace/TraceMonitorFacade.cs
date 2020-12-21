using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;

namespace RemoteApi.Trace
{
    public interface ITraceMonitorFacade
    {
        void Start();
    }
    
    public class TraceMonitorFacade : ITraceMonitorFacade
    {
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        private readonly IConsoleOperatorBootstrapper _consoleOperatorBootstrapper;
        
        public TraceMonitorFacade(
            IRemoteTraceMonitor remoteTraceMonitor,
            IConsoleOperatorBootstrapper consoleOperatorBootstrapper)
        {
            _remoteTraceMonitor = remoteTraceMonitor;
            _consoleOperatorBootstrapper = consoleOperatorBootstrapper;

            _consoleOperatorBootstrapper.Connected += ConnectedHandler;
            _remoteTraceMonitor.TextEntered += CommandEnteredHandler;
        }

        private void ConnectedHandler(string address, string cache)
        {
            _remoteTraceMonitor.SetPrompt(address);
            _remoteTraceMonitor.ClearTextBox();
            _remoteTraceMonitor.DisplayNextMessage(cache);
        }

        private void CommandEnteredHandler(string value)
        {
            CommandEnteredHandlerInternal(value);
        }

        private async Task CommandEnteredHandlerInternal(string value)
        {
            var result = await SafeExecution.TryCatchWithResultAsync(() => _consoleOperatorBootstrapper.ExecuteCommand(value),
                exception => { });

            if (!result.Item1)
            {
                throw new Exception("fail to send");
            }
        }

        public void Start()
        {
            _remoteTraceMonitor.Start();
        }
    }
}