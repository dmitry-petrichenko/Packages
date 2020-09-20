using System.Net.Sockets;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    public interface ISocketFactory
    {
        ISocket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}