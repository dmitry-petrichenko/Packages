using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.NetworkNode.SessionTCP.Factories
{
    public interface IInstructionReceiverFactory
    {
        IInstructionReceiver Create(string address);
    }
    
    public class DefaultInstructionReceiverFactory : IInstructionReceiverFactory
    {
        private readonly IRecorder _recorder;
        
        public DefaultInstructionReceiverFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public IInstructionReceiver Create(string address)
        {
            var networkAddress = new NetworkAddress(address);
            var socketFactory = new SocketFactory();
            
            var networkPoint = new NetworkPoint(
                networkAddress, 
                NetworkTunnelFactory, 
                socketFactory, 
                _recorder);
            
            var nodeGateWay = new NodeGateway(
                networkPoint, 
                SessionFactory,
                _recorder);
            
            var instructionReceiver = new InstructionReceiver(
                nodeGateWay,
                new SessionHolder(_recorder), 
                _recorder);

            return instructionReceiver;
        }
        
        private ISession SessionFactory(INetworkTunnel networkTunnel)
        {
            return new Session(networkTunnel, _recorder);
        }

        private INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
    }
}