using System;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface INodeGateway
    {
        event Action<ISession> ConnectionReceived;
    }

    public class NodeGateway : INodeGateway
    {
        private readonly INetworkPoint _networkPoint;
        private readonly Func<INetworkTunnel, ISession> _sessionFactory;
        private readonly IRecorder _recorder;
        
        public event Action<ISession> ConnectionReceived;
        
        public NodeGateway(
            INetworkPoint networkPoint, 
            Func<INetworkTunnel, ISession> sessionFactory,
            IRecorder recorder)
        {
            _networkPoint = networkPoint;
            _sessionFactory = sessionFactory;
            _networkPoint.Accepted += AcceptedHandler;
            _recorder = recorder;
        }

        private void AcceptedHandler(INetworkTunnel tunnel)
        {
            SafeExecution.TryCatch(() => AcceptedHandlerInternal(tunnel), ExceptionHandler);
        }
        
        private void AcceptedHandlerInternal(INetworkTunnel tunnel)
        {
            var session = _sessionFactory.Invoke(tunnel);
            ConnectionReceived?.Invoke(session);
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(NetworkConnector), exception.Message);
        }
    }
}