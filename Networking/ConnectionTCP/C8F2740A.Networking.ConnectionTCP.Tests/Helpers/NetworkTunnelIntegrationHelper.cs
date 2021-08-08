using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace SegmentedSocketTests.Helpers
{
    public static class NetworkTunnelIntegrationHelper
    {
        public static async Task<(INetworkTunnel, INetworkTunnel)> ArrangeNetworkTunnelTwoSides(string addressString, IRecorder recorder)
        {
            INetworkTunnel NetworkTunnelFactory(ISegmentedSocket socket)
                => new NetworkTunnel(socket, recorder);

            var address = new NetworkAddress(addressString);
            var networkPoint = new NetworkPoint(address, NetworkTunnelFactory, SocketFactory, recorder);
            var networkConnector = new NetworkConnector(NetworkTunnelFactory, SocketFactory, recorder);
            
            var acceptedTask = new TaskCompletionSource<bool>();
            INetworkTunnel acceptedTunnel1 = default;
            networkPoint.Accepted += tunnel2 =>
            {
                acceptedTunnel1 = tunnel2;
                acceptedTask.SetResult(true);
            };
            
            INetworkTunnel connectedTunnel2 = default;
            var connectTask = Task.Run(() =>
            {
                networkConnector.TryConnect(address, out connectedTunnel2);
            });
            
            await Task.WhenAll(acceptedTask.Task, connectTask);

            return (acceptedTunnel1, connectedTunnel2);
        }
        
        private static ISegmentedSocket SocketFactory(
            AddressFamily addressFamily,
            SocketType socketType,
            ProtocolType protocolType,
            string tag)
        {
            var socket = new SocketAbstraction(addressFamily, socketType, protocolType, tag);
            var dataSplitterFactory = new DataSplitterFactory();
            
            return new SegmentedSocket(socket, dataSplitterFactory);
        }
    }
}