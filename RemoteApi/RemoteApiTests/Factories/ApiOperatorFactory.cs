using RemoteApi.Monitor;

namespace RemoteApi.Factories
{
    public interface IApiOperatorFactory
    {
        IApiOperator Create(string address);
    }
    
    public class ApiOperatorFactory : IApiOperatorFactory
    {
        private readonly ISystemRecorder _systemRecorder;
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        private readonly IMonitoredRemoteOperatorFactory _monitoredRemoteOperatorFactory;
        private readonly ITraceableRemoteApiMapFactory _traceableRemoteApiMapFactory;
        private readonly IApplicationRecorder _applicationRecorder;
        
        public ApiOperatorFactory(
            ISystemRecorder systemRecorder,
            IMonitoredRemoteOperatorFactory monitoredRemoteOperatorFactory,
            ITraceableRemoteApiMapFactory traceableRemoteApiMapFactory,
            IApplicationRecorder applicationRecorder)
        {
            _systemRecorder = systemRecorder;
            _traceableRemoteApiMapFactory = traceableRemoteApiMapFactory;
            _monitoredRemoteOperatorFactory = monitoredRemoteOperatorFactory;
            _applicationRecorder = applicationRecorder;
        }

        public IApiOperator Create(string address)
        {
            var monitoredRemoteOperator = _monitoredRemoteOperatorFactory.Create(address);
            var traceableRemoteApiMap = _traceableRemoteApiMapFactory.Create(address);
            
            return new ApiOperator(monitoredRemoteOperator, traceableRemoteApiMap, _applicationRecorder);
        }
    }
}