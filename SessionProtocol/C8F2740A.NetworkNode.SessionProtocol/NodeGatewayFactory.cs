using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface INodeGatewayFactory
    {
        INodeGateway Create(string address);
    }
    
    public class NodeGatewayFactory : INodeGatewayFactory
    {
        private IRecorder _recorder;
        
        public NodeGatewayFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public INodeGateway Create(string address)
        {
            var networkAddress = new NetworkAddress(address);
            var socketFactory = new SocketFactory();
            var networkPoint = new NetworkPoint(networkAddress, NetworkTunnelFactory, socketFactory, _recorder);
            
            return new NodeGateway(networkPoint, SessionFactory, _recorder);
        }
        
        private IReceiveSession SessionFactory(INetworkTunnel networkTunnel)
        {
            return new ReceiveSession(networkTunnel, _recorder);
        }

        private INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
    }
}