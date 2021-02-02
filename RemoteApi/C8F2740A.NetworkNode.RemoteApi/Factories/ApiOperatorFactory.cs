using RemoteApi.Monitor;

namespace RemoteApi.Factories
{
    public interface IApiOperatorFactory
    {
        IApiOperator Create(string address);
    }
    
    public class ApiOperatorFactory : IApiOperatorFactory
    {
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        private readonly IMonitoredRemoteOperatorFactory _monitoredRemoteOperatorFactory;
        private readonly ITraceableRemoteApiMapFactory _traceableRemoteApiMapFactory;
        private readonly IApplicationRecorder _applicationRecorder;
        
        public ApiOperatorFactory(
            IMonitoredRemoteOperatorFactory monitoredRemoteOperatorFactory,
            ITraceableRemoteApiMapFactory traceableRemoteApiMapFactory,
            IApplicationRecorder applicationRecorder)
        {
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