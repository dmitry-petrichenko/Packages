using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

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
        private readonly IRecorder _recorder;
        
        public TraceMonitorFacade(
            IRemoteTraceMonitor remoteTraceMonitor,
            IConsoleOperatorBootstrapper consoleOperatorBootstrapper,
            IRecorder recorder)
        {
            _remoteTraceMonitor = remoteTraceMonitor;
            _consoleOperatorBootstrapper = consoleOperatorBootstrapper;
            _recorder = recorder;

            _consoleOperatorBootstrapper.Connected += ConnectedHandler;
            _remoteTraceMonitor.TextEntered += CommandEnteredHandler;

            _consoleOperatorBootstrapper.InstructionReceived += InstructionReceivedHandler;
        }

        private IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> value)
        {
            _remoteTraceMonitor.DisplayNextMessage(value.ToText());
            return Enumerable.Empty<byte>();
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
                _recorder.RecordError("", result.Item2);
            }
        }

        public void Start()
        {
            _remoteTraceMonitor.Start();
            _consoleOperatorBootstrapper.Start();
        }
    }
}