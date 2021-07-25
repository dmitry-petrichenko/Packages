using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.SessionTCP.Impl;

namespace C8F2740A.NetworkNode.SessionTCP.Factories
{
    public interface IInstructionReceiverFactory
    {
        IInstructionReceiver Create(string address);
    }
    
    public class BaseInstructionReceiverFactory : IInstructionReceiverFactory
    {
        private readonly IRecorder _recorder;
        
        public BaseInstructionReceiverFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public IInstructionReceiver Create(string address)
        {
            var networkAddress = new NetworkAddress(address);

            var networkPoint = new NetworkPoint(
                networkAddress, 
                NetworkTunnelFactory, 
                SocketFactory, 
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
        
        protected virtual ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            return new SocketAbstraction(addressFamily, socketType, protocolType, tag);
        }
        
        protected virtual ISession SessionFactory(INetworkTunnel networkTunnel)
        {
            return new Session(networkTunnel, _recorder);
        }

        protected virtual INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
    }
}