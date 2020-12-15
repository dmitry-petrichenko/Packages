using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.NetworkNode.SessionTCP.Factories
{
    public class DefaultInstructionReceiverFactory
    {
        private readonly IRecorder _recorder;
        
        public DefaultInstructionReceiverFactory()
        {
            _recorder = new DefaultRecorder(new RecorderSettings());
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