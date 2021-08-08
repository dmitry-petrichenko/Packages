using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace C8F2740A.NetworkNode.RemoteApi.Factories
{
    public interface IMonitoredRemoteOperatorFactory
    {
        IMonitoredRemoteOperator Create(string address);
    }
    
    public class BaseMonitoredRemoteOperatorFactory : IMonitoredRemoteOperatorFactory
    {
        private IInstructionSenderFactory _instructionSenderFactory;
        private IRemoteTraceMonitorСonsistent _remoteTraceMonitor;
        private IApplicationRecorder _applicationRecorder;
        private IRecorder _recorder;
        
        public BaseMonitoredRemoteOperatorFactory(
            IInstructionSenderFactory instructionSenderFactory,
            IRemoteTraceMonitorСonsistent remoteTraceMonitor,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _instructionSenderFactory = instructionSenderFactory;
            _remoteTraceMonitor = remoteTraceMonitor;
            _applicationRecorder = applicationRecorder;
            _recorder = recorder;
        }

        public IMonitoredRemoteOperator Create(string address)
        {
            var instructionSenderHolder = new InstructionSenderHolder(_recorder);
            var remoteApiOperator = new RemoteApiOperator(
                instructionSenderHolder,
                _instructionSenderFactory,
                _applicationRecorder,
                _recorder);
            var connectParser = new ConnectParser(remoteApiOperator, _recorder);
            var autoLocalConnector = new AutoLocalConnector(connectParser, _recorder, address);
            var monitoredRemoteOperator = new MonitoredRemoteOperator(autoLocalConnector, _remoteTraceMonitor, _recorder);

            return monitoredRemoteOperator;
        }
    }
}