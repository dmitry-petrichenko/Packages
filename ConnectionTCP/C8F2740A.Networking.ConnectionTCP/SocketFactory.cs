using System.Net.Sockets;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    public class SocketFactory : ISocketFactory
    {
        public ISocket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new SocketAbstraction(addressFamily, socketType, protocolType);
        }
    }
}