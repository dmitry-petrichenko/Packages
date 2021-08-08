using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;
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
        
        protected virtual ISegmentedSocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            var socketAbstraction = new SocketAbstraction(addressFamily, socketType, protocolType, tag);
            var segmentedSocket = new SegmentedSocket(socketAbstraction, new DataSplitterFactory()); // TODO extract factory
            
            return segmentedSocket;
        }

        protected virtual ISession SessionFactory(INetworkTunnel tunnel)
        {
            return new Session(tunnel, _recorder);
        }
        
        protected virtual INetworkTunnel NetworkTunnelFactory(ISegmentedSocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
    }
}