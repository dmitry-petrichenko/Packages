using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface ITransmitSessionFactory
    {
        ITransmitSession Create();
    }
    
    public class TransmitSessionFactory : ITransmitSessionFactory
    {
        private readonly IRecorder _recorder;

        public TransmitSessionFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }
        
        private INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }

        public ITransmitSession Create()
        {
            var networkConnector = new NetworkConnector(NetworkTunnelFactory, new SocketFactory());
            var transmitSession = new TransmitSession(networkConnector, new NetworkAddressFactory(), _recorder);

            return transmitSession;
        }
    }
}