using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.SessionTCP.Impl;

namespace C8F2740A.NetworkNode.SessionTCP.Factories
{
    public interface IInstructionSenderFactory
    {
        IInstructionSender Create(string address);
    }
    
    public class BaseInstructionSenderFactory : IInstructionSenderFactory
    {
        private readonly IRecorder _recorder;
        
        public BaseInstructionSenderFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public IInstructionSender Create(string address)
        {
            var networkAddress = new NetworkAddress(address);
            var networkConnector = new NetworkConnector(
                NetworkTunnelFactory,
                SocketFactory,
                _recorder);
            
            var nodeVisitor = new NodeVisitor(networkConnector, SessionFactory, _recorder);
            
            var instructionSender = new InstructionSender(
                nodeVisitor,
                networkAddress,
                new SessionHolder(_recorder), 
                _recorder);

            return instructionSender;
        }
        
        protected virtual ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            return new SocketAbstraction(addressFamily, socketType, protocolType);
        }

        protected virtual ISession SessionFactory(INetworkTunnel tunnel)
        {
            return new Session(tunnel, _recorder);
        }
        
        protected virtual INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
    }
}