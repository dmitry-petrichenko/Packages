using System;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface INodeVisitor : IDisposable
    {
        (bool, ISession) TryConnect(INetworkAddress networkAddress);
    }

    public class NodeVisitor : INodeVisitor
    {
        private readonly INetworkConnector _networkConnector;
        private readonly IRecorder _recorder;
        
        public NodeVisitor(
            INetworkConnector networkConnector,
            IRecorder recorder)
        {
            _networkConnector = networkConnector;
            _recorder = recorder;
        }

        public void Dispose()
        {
        }

        public (bool, ISession) TryConnect(INetworkAddress networkAddress)
        {
            return SafeExecution.TryCatchWithResult(() => TryConnectInternal(networkAddress), ExceptionHandler);
        }
        
        private (bool, ISession) TryConnectInternal(INetworkAddress networkAddress)
        {
            if (_networkConnector.TryConnect(networkAddress,
                out INetworkTunnel tunnel))
            {
                return (true, new Session(tunnel, _recorder));
            }

            _recorder.RecordError(GetType().Name, "Fail connect to remote address");
            return (false, default);
        }
        
        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(GetType().Name, exception.Message);
        }
    }
}