using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Operator;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace C8F2740A.NetworkNode.RemoteApi.Factories
{
    public interface IApiOperatorFactory
    {
        IApiOperator Create(string address);
    }
    
    public class ApiOperatorFactory : IApiOperatorFactory
    {
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