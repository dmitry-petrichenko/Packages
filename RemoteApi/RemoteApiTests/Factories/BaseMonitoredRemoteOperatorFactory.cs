using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi.Monitor;
using RemoteApi.Trace;

namespace RemoteApi.Factories
{
    public interface IMonitoredRemoteOperatorFactory
    {
        IMonitoredRemoteOperator Create(string address);
    }
    
    public class BaseMonitoredRemoteOperatorFactory : IMonitoredRemoteOperatorFactory
    {
        private IInstructionSenderFactory _instructionSenderFactory;
        private IRemoteTraceMonitor _remoteTraceMonitor;
        private IRecorder _recorder;
        
        public BaseMonitoredRemoteOperatorFactory(
            IInstructionSenderFactory instructionSenderFactory,
            IRemoteTraceMonitor remoteTraceMonitor,
            IRecorder recorder)
        {
            _instructionSenderFactory = instructionSenderFactory;
            _remoteTraceMonitor = remoteTraceMonitor;
            _recorder = recorder;
        }

        public IMonitoredRemoteOperator Create(string address)
        {
            var instructionSenderHolder = new InstructionSenderHolder(_recorder);
            var remoteApiOperator = new RemoteApiOperator(
                instructionSenderHolder,
                _instructionSenderFactory,
                _recorder);
            var connectParser = new ConnectParser(remoteApiOperator, _recorder);
            var autoLocalConnector = new AutoLocalConnector(connectParser, _recorder, address);
            var monitoredRemoteOperator = new MonitoredRemoteOperator(autoLocalConnector, _remoteTraceMonitor, _recorder);

            return monitoredRemoteOperator;
        }
    }
}