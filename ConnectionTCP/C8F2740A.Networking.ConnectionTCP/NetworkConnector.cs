using System;
using System.Net.Sockets;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    public interface INetworkConnector
    {
        bool TryConnect(INetworkAddress networkAddress, out INetworkTunnel networkTunnel);
    }
    
    public class NetworkConnector : INetworkConnector
    {
        private readonly Func<ISocket, INetworkTunnel> _networkTunnelFactory;
        private readonly Func<AddressFamily, SocketType, ProtocolType, ISocket> _socketFactory;
        private readonly IRecorder _recorder;

        public NetworkConnector(
            Func<ISocket, INetworkTunnel> networkTunnelFactory, 
            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory,
            IRecorder recorder)
        {
            _socketFactory = socketFactory;
            _networkTunnelFactory = networkTunnelFactory;
            _recorder = recorder;
        }

        public bool TryConnect(INetworkAddress networkAddress, out INetworkTunnel networkTunnel)
        {
            bool result = false;
            networkTunnel = default;
            
            try
            {
                result = TryConnectInternal(networkAddress, out networkTunnel);
            }
            catch(Exception exception)
            {
                ExceptionHandler(exception);
            }
            
            return result;
        }

        private bool TryConnectInternal(INetworkAddress networkAddress, out INetworkTunnel networkTunnel)
        {
            var socket = _socketFactory.Invoke(networkAddress.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var isFail = false;
            networkTunnel = default;
            
            SafeExecution.TryCatch(
                () => socket.Connect(networkAddress.IP, networkAddress.Port),
                e =>
                {
                    SafeExecution.TryCatch(() => socket.Dispose(),
                        e => ExceptionHandler(e));
                    isFail = true;
                });

            if (!isFail)
                networkTunnel = _networkTunnelFactory.Invoke(socket);
            
            return !isFail;
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(NetworkConnector), exception.Message);
        }
    }
}