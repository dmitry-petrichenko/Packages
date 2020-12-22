using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.NetworkNode.SessionTCP.Factories
{
    public interface IInstructionSenderFactory
    {
        IInstructionSender Create(string address);
    }
    
    public class DefaultInstructionSenderFactory : IInstructionSenderFactory
    {
        private readonly IRecorder _recorder;
        
        public DefaultInstructionSenderFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public IInstructionSender Create(string address)
        {
            var socketFactory = new SocketFactory();
            var networkAddress = new NetworkAddress(address);
            var networkConnector = new NetworkConnector(
                NetworkTunnelFactory,
                socketFactory);
            
            var nodeVisitor = new NodeVisitor(networkConnector, SessionFactory, _recorder);
            
            var instructionSender = new InstructionSender(
                nodeVisitor,
                networkAddress,
                new SessionHolder(_recorder), 
                _recorder);

            return instructionSender;
        }

        private ISession SessionFactory(INetworkTunnel tunnel)
        {
            return new Session(tunnel, _recorder);
        }
        
        private INetworkTunnel NetworkTunnelFactory(ISocket socket)
        {
            return new NetworkTunnel(socket, _recorder);
        }
        
        private class RecorderSettings : IRecorderSettings
        {
            public RecorderSettings()
            {
                ShowErrors = true;
                ShowInfo = true;
            }

            public bool ShowErrors { get; }
            public bool ShowInfo { get; }
        }
    }
}