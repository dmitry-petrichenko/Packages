using System;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace C8F2740A.NetworkNode.RemoteApi.Operator
{
    public interface IApiOperator
    {
        event Action Finished;
    }
    
    public class ApiOperator : IApiOperator
    {
        private readonly IMonitoredRemoteOperator _monitoredRemoteOperator;
        private readonly ITraceableRemoteApiMap _traceableRemoteApiMap;
        private readonly IApplicationRecorder _applicationRecorder;
        
        public event Action Finished;
        
        public ApiOperator(
            IMonitoredRemoteOperator monitoredRemoteOperator,
            ITraceableRemoteApiMap traceableRemoteApiMap,
            IApplicationRecorder applicationRecorder)
        {
            _traceableRemoteApiMap = traceableRemoteApiMap;
            _monitoredRemoteOperator = monitoredRemoteOperator;
            _applicationRecorder = applicationRecorder;
            
            _traceableRemoteApiMap.RegisterWrongCommandHandler(WrongCommandHandler);
            _monitoredRemoteOperator.Finished += FinishedHandler;
            _monitoredRemoteOperator.Start();
        }

        private void FinishedHandler()
        {
            Finished?.Invoke();
        }

        private void WrongCommandHandler()
        {
            _applicationRecorder.RecordInfo("Local", "Wrong command");
        }
    }
}