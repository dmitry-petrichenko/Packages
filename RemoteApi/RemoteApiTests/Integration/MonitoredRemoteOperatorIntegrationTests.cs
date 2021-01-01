using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi.Trace;

namespace RemoteApi.Integration
{
    public class MonitoredRemoteOperatorIntegrationTests
    {
        private readonly IMonitoredRemoteOperator _mro;
        private readonly ITraceableRemoteApiMap _tram;
        
        public MonitoredRemoteOperatorIntegrationTests()
        {
            //_mro = new MonitoredRemoteOperator(autoLocalConnector, _remoteTraceMonitor, recorder);
            //_tram = new TraceableRemoteApiMap(remoteApiMap, remoteRecorderSender, recorder);
        }

        private IInstructionSenderFactory CreateInstructionSender(ISocketFactory socketFactory)
        {
            
        }
    }
}