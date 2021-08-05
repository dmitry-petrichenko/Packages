using System.Net.Sockets;

namespace C8F2740A.Networking.ConnectionTCP.Network.Sockets
{
    public interface ISocketFactory
    {
        ISocket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}