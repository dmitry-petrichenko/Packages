using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface INodeGateway : IDisposable
    {
        event Action<byte[]> CommandReceived;

        void SendCommand(byte[] command);
    }
    
    public class NodeGateway : INodeGateway
    {
        private readonly INetworkPoint _networkPoint;
        private readonly Func<INetworkTunnel, IReceiveSession> _sessionFactory;
        private readonly IRecorder _recorder;
        
        public event Action<byte[]> CommandReceived;

        private IReceiveSession _currentReceiveSession;
        
        public NodeGateway(INetworkPoint networkPoint, Func<INetworkTunnel, IReceiveSession> sessionFactory, IRecorder recorder)
        {
            _recorder = recorder;
            _sessionFactory = sessionFactory;
            _networkPoint = networkPoint;
            _networkPoint.Accepted += ConnectionAcceptedHandler;
        }
        
        public void Dispose()
        {
            RemoveCurrentSession();
        }
        
        public void SendCommand(byte[] command)
        {
            _currentReceiveSession?.SendCommand(command);
        }

        private void ConnectionAcceptedHandler(INetworkTunnel networkTunnel)
        {
            SafeExecution.TryCatch(() => ConnectionAcceptedHandlerInternal(networkTunnel), ExceptionHandler);
        }

        private void ConnectionAcceptedHandlerInternal(INetworkTunnel networkTunnel)
        {
            _networkPoint.Accepted -= ConnectionAcceptedHandler;
            SafeExecution.TryCatchSuccessAsync(ConnectionAcceptedHandlerAsync(networkTunnel), ExceptionHandler, ConnectionAcceptedFinish);

            void ConnectionAcceptedFinish()
            {
                _networkPoint.Accepted += ConnectionAcceptedHandler;
            }
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(NodeGateway), exception.Message);
            RemoveCurrentSession();
        }

        private async Task ConnectionAcceptedHandlerAsync(INetworkTunnel networkTunnel)
        {
            var session = _sessionFactory.Invoke(networkTunnel);
            _recorder.RecordInfo(nameof(NodeGateway), "Connection accepted");
            
            var valid= await session.Validate();
            _recorder.RecordInfo(nameof(NodeGateway), $"Validation complete with result: {valid}");
            
            if (!valid)
            {
                session.Dispose();
                return;
            }

            UpdateCurrentSession(session);
        }

        private void ReceivedBytesHandler(byte[] bytes)
        {
            CommandReceived?.Invoke(bytes);
        }
        
        private void SessionClosedHandler()
        {
            RemoveCurrentSession();
        }
        
        private void UpdateCurrentSession(IReceiveSession receiveSession)
        {
            RemoveCurrentSession();
            AddSession(receiveSession);
        }

        private void AddSession(IReceiveSession receiveSession)
        {
            _currentReceiveSession = receiveSession;
            receiveSession.Received += ReceivedBytesHandler;
            receiveSession.Closed += SessionClosedHandler;
        }
        
        private void RemoveSession(IReceiveSession receiveSession)
        {
            receiveSession.Received -= ReceivedBytesHandler;
            receiveSession.Closed -= SessionClosedHandler;
            receiveSession.Dispose();
        }

        private void RemoveCurrentSession()
        {
            if (_currentReceiveSession != default)
            {
                RemoveSession(_currentReceiveSession);
                _currentReceiveSession = default;
            }
        }
    }
}